using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; 
public class CarManager : MonoBehaviour
{
    //Enregistrer la position de chaque voiture à intervalle de temps régulier sur tau_max. Dans le compute shader calculer l'intersection entre la direciton de la voiture au temps t-t_i et la hitbox de chacune des voitures au temps t-tau_i (produit scalaire) devant la notre et dans le champ de vision.
    //Appliquer le modèle de premier ordre avec cette intersection. 
    public static CarManager current; 

    [SerializeField] private ComputeShader carComputeShader;
    private List<GameObject> cars = new List<GameObject>();

    [SerializeField] private GameObject explosionPrefab;
    
    public float securityDistance;
    [SerializeField] private float averageReactionTime;
    
    public float maxSpeed;
    [SerializeField] private AnimationCurve leaderSpeed;

    [SerializeField] private GameObject tracePrefab;
    private int recordFramePeriod = 1;
    private int recordFrameTimer = 1;


   
    private Vector2[] histories; //files bornées (historiques) accolées
    private int[] historiesQueueStart; 
    private int[] historiesQueueEnd;
    private int historySize;

    private float maxTau; //Temps de réaction maximal
    private List<VehicleData> priorCarList;
    private VehicleData[] data;
    private float count;

    public bool started { get; private set; }
    private bool showTraces = false;
    private GameObject traceRoot;

    private int clickedCar = -1;

    private void Start()
    {
        priorCarList = new List<VehicleData>();
        if(current == null)
        {
            current = this;
        }
    }
    public void StartSimulation()
    {
 
        data = priorCarList.ToArray();
        historySize = Mathf.FloorToInt(maxTau / (Time.fixedDeltaTime * recordFramePeriod)) + 2;

        histories = new Vector2[data.Length * historySize];
        historiesQueueStart = new int[data.Length];
        historiesQueueEnd = new int[data.Length];
        for(int i = 0; i < historiesQueueEnd.Length; i++)
        {
            historiesQueueEnd[i] = 1;
        }
        for(int i = 0; i < cars.Count; i++)
        {
            for(int j = 0; j < historySize; j++)
            {
                RecordPosition(i, cars[i].transform.position);
            }
        }
        started = true;
    }
    
    public void CreateCar(Vector2 position, VehicleData vehicleData, GameObject vehicleSprite, bool hasDestination)
    {
        GameObject car = Instantiate(vehicleSprite, position, Quaternion.identity);
        vehicleData.roadPointID = PathManager.current.road.GetClosestPointID(position);
        vehicleData.target = PathManager.current.road.GetPointByID(vehicleData.roadPointID).position;
        vehicleData.position = position;
        if(!hasDestination)
        {
            vehicleData.destination = new Vector2(Random.Range(-1000, 1000), Random.Range(-1000, 1000));
        }
        
        vehicleData.moving = 1;
        vehicleData.crashedWithCar = -1;
        priorCarList.Add(vehicleData);
        maxTau = Mathf.Max(vehicleData.reactionTime, maxTau);
        
        cars.Add(car);
    }
    public void RemoveLastCar()
    {
        priorCarList.RemoveAt(priorCarList.Count - 1);
        cars.RemoveAt(cars.Count - 1);
    }
    private void Update()
    {
        if(started)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Input.GetButtonDown("Fire1"))
            {
                for (int i = 0; i < cars.Count; i++)
                {
                    if (Vector2.Distance(mousePos, cars[i].transform.position) < 1f)
                    {
                        clickedCar = i;
                    }
                }
            }
            else if(Input.GetButtonUp("Fire1"))
            {
                clickedCar = -1;
            }
        }
        
    }
    private void FixedUpdate()
    {
        if (started)
        {
            
            recordFrameTimer -= 1;
            if(recordFrameTimer == 0)
            {
                RecordPositions();
                recordFrameTimer = recordFramePeriod;
            }
            
            CalculateMovements();
            MoveCars();
            if(showTraces)
            {
                ShowTraces();
            }
            
            
        }
    }
    private void ShowTraces()
    {
        if(traceRoot != null)
        {
            Destroy(traceRoot);
        }
        traceRoot = new GameObject("TraceRoot");
        for(int i = 0; i < historySize; i++)
        {
            GameObject trace = Instantiate(tracePrefab, GetPositionInHistory(0, i), Quaternion.identity);
            trace.transform.SetParent(traceRoot.transform);
        }
    }
    
    private void RecordPositions()
    {
        for (int i = 0; i < cars.Count; i++)
        {
            RecordPosition(i, cars[i].transform.position);
        }
        
    }
    
    private void RecordPosition(int i, Vector2 v)
    {
       
        //Ajout dans une file bornée
        int queueStart = historiesQueueStart[i];
        int queueEnd = historiesQueueEnd[i]; //queueEnd pointe sur l'endroit où le prochain élément va être ajouté
        
        if (queueEnd == queueStart) //Si la fin est confondue avec le début on va écraser le début et décaler le marqueur de début
        {
            
            historiesQueueStart[i] = mod(queueStart + 1, historySize);
        }
        int indexToAddInQueue = queueEnd;
        
        historiesQueueEnd[i] = mod(indexToAddInQueue + 1,historySize); //on décale le marqueur de fin

        
        histories[i * historySize + indexToAddInQueue] = v;

    }
    private void CalculateMovements()
    {
        
        int vector2Size = sizeof(float) * 2;
        int totalSize = 3 * vector2Size + 5 * sizeof(float) + 4 * sizeof(int);

        ComputeBuffer carsBuffer = new ComputeBuffer(data.Length, totalSize);
        carsBuffer.SetData(data);
        carComputeShader.SetBuffer(0, "cars", carsBuffer);

        ComputeBuffer historiesBuffer = new ComputeBuffer(histories.Length, vector2Size);
        historiesBuffer.SetData(histories);
        carComputeShader.SetBuffer(0, "histories", historiesBuffer);

        ComputeBuffer historiesQueueStartBuffer = new ComputeBuffer(historiesQueueStart.Length, sizeof(int));
        historiesQueueStartBuffer.SetData(historiesQueueStart);
        carComputeShader.SetBuffer(0, "historiesQueueStart", historiesQueueStartBuffer);

        ComputeBuffer historiesQueueEndBuffer = new ComputeBuffer(historiesQueueEnd.Length, sizeof(int));
        historiesQueueEndBuffer.SetData(historiesQueueEnd);
        carComputeShader.SetBuffer(0, "historiesQueueEnd", historiesQueueEndBuffer);

        carComputeShader.SetInt("numCars", data.Length);
        carComputeShader.SetFloat("deltaTime", Time.deltaTime);

        carComputeShader.SetInt("historySize", historySize);
        carComputeShader.SetFloat("time", Time.fixedTime);
        carComputeShader.SetBuffer(0, "histories", historiesBuffer);
        carComputeShader.SetFloat("maxSpeed", maxSpeed);
        carComputeShader.SetFloat("securityDistance", securityDistance);
        carComputeShader.SetFloat("recordDelta", Time.fixedDeltaTime * recordFramePeriod);
        carComputeShader.Dispatch(0, data.Length, 1, 1);

        carsBuffer.GetData(data);
        carsBuffer.Dispose();
        historiesBuffer.Dispose();
        historiesQueueStartBuffer.Dispose();
        historiesQueueEndBuffer.Dispose();
    }
    private void MoveCars()
    {
        
        Debug.DrawLine(cars[0].transform.position, GetPositionDelayed(0, 0.25f));
        

        for (int i = 0; i < cars.Count; i++)
        {
            if(data[i].moving == 0)
            {
                if(Vector2.Distance(data[i].position, data[i].destination) < 0.05f)
                {
                    StopSimulation();
                    Destroy(cars[i]); 
                    priorCarList.RemoveAt(i);
                    StartSimulation();
                }
            }
            if(data[i].crashedWithCar > -1)
            {
                data[i].moving = 0;
                Explode(data[i].position);
                
                data[i].crashedWithCar = -1;
            }
            if(i != clickedCar)
            {
                cars[i].transform.position = data[i].position;
            }
            else
            {
                data[i].position = cars[i].transform.position;
            }
            
            if (data[i].hasToChangePoint == 1)
            {

                try
                {
                    data[i].roadPointID = PathManager.current.road.GetClosestPointIDAmongSet(data[i].destination, PathManager.current.road.GetPointByID(data[i].roadPointID).neighbours);
                    data[i].target = PathManager.current.road.GetPointByID(data[i].roadPointID).position;
                    
                }
                catch
                {
                    data[i].moving = 0;
                }
                data[i].hasToChangePoint = 0;
            }
            data[i].speed = Mathf.Min(maxSpeed, data[i].maxCarSpeed);
            
            cars[i].transform.right = data[i].target - data[i].position;
        }
    }
    
    public void StopSimulation()
    {
        started = false;
        priorCarList = data.ToList();
    }
    private Vector2 GetPositionInHistory(int car, int i)
    {
        int queueStart = historiesQueueStart[car];
        int indexInQueue = (queueStart + i) % historySize;
        
        return histories[historySize * car + indexInQueue];
    }
    private Vector2 GetPositionDelayed(int car, float tau)
    {
        int end = historiesQueueEnd[car];
        
        int indexInSubArray = mod(Mathf.FloorToInt(end - tau / (Time.fixedDeltaTime * recordFramePeriod)), historySize);

        return histories[car * historySize + indexInSubArray];
    }
    private void Explode(Vector2 position)
    {
        Instantiate(explosionPrefab, position, Quaternion.identity);
    }
    int mod(int x, int m)
    {
        
        return (x % m + m) % m;
    }
}
