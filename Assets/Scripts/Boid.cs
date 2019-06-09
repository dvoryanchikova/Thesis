using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour {    

	public Vector3 velocity, v2_separation, v3_alignment;
	public float maxSpeed = 7f;
	public float distance, separationCount;
	private float sight;
	Vector3 centerMass, mouse;
	public List<BoxCollider> boids = new List<BoxCollider>();
    private Boid alarmObjectBoid = null;
    public Transform movementTarget;
    public float coefForAlarmVelocity = 500;
    public float coefForChaseelocity = 500;
    private Vector3 v1_cohesion, v4_target, v5_opositeAlarmDirection, v6_chaseDirection;    

	
    public Transform chaseTarget = null;
    
    public float distanceToGetAlarm;

    public bool isLeader = false;
    public Boid leaderBoid;
   
    public List<string> tagsToFearSomeone;
    public float timeForAlarm = 3f;
    private Coroutine alarmCoroutine;

    public List<string> tagsToKillSomeone;
    public float resetTimeForChase = 3f;
    Coroutine chaseCoroutine = null;

    public Color myColor;


    public float meshResolution;
    public int edgeResolveIterations;
    public float edgeDstThreshold;

    public float maskCutawayDst = .1f;

    public MeshFilter viewMeshFilter;
    Mesh viewMesh;

    bool isStuned = false;
    public float stunTime = 5f;

    public bool isDead = false;

    private void Start()
	{
		InvokeRepeating("Velocity", 0, 0.2f);
        //target = GameObject.Find("Target").transform;
        v5_opositeAlarmDirection = Vector3.zero;
        myColor = GetComponent<MeshRenderer>().material.color;

        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
    }

    void Velocity()
	{
        if (isStuned)
        {
            return;
        }

        velocity = Vector3.zero;

        Rule1 (boids);
		Rule2 (boids);
		Rule3 (boids);
		Rule4 ();
        Rule5();
        Rule6();
        Rule7();
        Rule8();

        velocity += v1_cohesion + v2_separation + 1.5f * v3_alignment + 2*v4_target + v5_opositeAlarmDirection + v6_chaseDirection;

        velocity = Vector3.ClampMagnitude (velocity, maxSpeed);
        velocity.y = 0f;


	}

	void Update()
	{
        if (isStuned)
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            return;
        }
        
		//Velocity ();
		if (transform.position.magnitude > 25)
		{
			velocity += -transform.position.normalized;
		}        
        transform.position += velocity * Time.deltaTime;        

        //Debug.DrawRay(transform.position, cohesion, Color.magenta);        
    }

    void LateUpdate()
    {
        DrawFieldOfView();
    }

    void Rule1(List<BoxCollider> birds)
	{
		v1_cohesion = Vector3.zero;
		foreach(var bird in birds)
		{
            if(bird!=null)
			v1_cohesion += bird.transform.position;
		}
		centerMass = v1_cohesion / birds.Count;
		v1_cohesion = centerMass - transform.position;
		v1_cohesion = Vector3.ClampMagnitude(v1_cohesion, maxSpeed);

	}

	void Rule2(List<BoxCollider> birds)
	{
		v2_separation = Vector3.zero;
        separationCount = 0;

        foreach (var bird in birds)
		{
			if(bird != null && bird.gameObject != gameObject && (bird.transform.position - transform.position).magnitude < distance)
			{
                v2_separation += (transform.position - bird.transform.position);// / (transform.position - bird.transform.position).magnitude;
				//separationCount++;
			}
		}
		if (separationCount > 0)
		{
			//v2_separation = v2_separation / separationCount;
			//v2_separation = Vector3.ClampMagnitude(v2_separation, maxSpeed);
		}
	}

	void Rule3(List<BoxCollider> birds)
	{
		v3_alignment = Vector3.zero;
		foreach(var bird in birds)
		{
            if (bird!=null)
            //Debug.Log(bird.name);
				v3_alignment += bird.GetComponent<Boid> ().velocity;
		}
		v3_alignment /= birds.Count;
		v3_alignment = Vector3.ClampMagnitude(v3_alignment, maxSpeed);

	}

	void Rule4()
	{
        //mouse = new Vector3(Camera.main.ScreenToWorldPoint (Input.mousePosition).x,Camera.main.ScreenToWorldPoint (Input.mousePosition).y, 0f);
        //v4_target = Vector3.zero;
        Vector3 targetPos = chaseTarget != null ? chaseTarget.position : movementTarget.position;
		v4_target = targetPos - centerMass;
		v4_target = Vector3.ClampMagnitude (v4_target, maxSpeed);
        targetPos.y = transform.position.y;
        transform.LookAt(targetPos);
    }

    void Rule5()
    {
        if (alarmObjectBoid == null)
        {
            v5_opositeAlarmDirection = Vector3.zero;
        }
        else
        {
            v5_opositeAlarmDirection = transform.position - alarmObjectBoid.transform.position;
            v5_opositeAlarmDirection.y = 0;
            v5_opositeAlarmDirection.Normalize();
            v5_opositeAlarmDirection *= coefForAlarmVelocity;
        }
    }

    void Rule6()
    {
        if (chaseTarget == null)
        {
            v6_chaseDirection = Vector3.zero;
        }
        else
        {
            v6_chaseDirection = chaseTarget.position - transform.position;
            v6_chaseDirection.y = 0;
            v6_chaseDirection.Normalize();
            v6_chaseDirection *= coefForChaseelocity;
        }
    }

    private void RemoveStun()
    {
        isStuned = false;
    }

    public void ReactToHitStun()
    {
        isStuned = true;
        Invoke("RemoveStun", stunTime);
    }    

    public void ReactToHitKill()
    {
        if (!isDead)
        {
            isDead = true;
            MakeAlarm(this);
            Die();
        }
    }

    public void Die()
    {
        StartCoroutine(DieCor());
    }

    private IEnumerator DieCor()
    {
        this.transform.Rotate(-75, 0, 0);
        BoxCollider thisBc = GetComponent<BoxCollider>();
        foreach (BoxCollider boid in boids)
        {
            if (boid != null && boid != thisBc)
                boid.GetComponent<Boid>().boids.Remove(thisBc);
        }        
        yield return new WaitForSeconds(1.5f);

        Destroy(this.gameObject);

    }

    private void MakeAlarm(Boid alarmBoid)
    {
        if (alarmBoid != null)
        {
            foreach (BoxCollider boid in boids)
            {
                if (boid != null && boid.gameObject != gameObject)
                    boid.gameObject.GetComponent<Boid>().GetAlarm(alarmBoid);
            }
        }
    }

    public void GetAlarm(Boid alarmBoid)
    {        
        if (Vector3.Distance(transform.position, alarmBoid.transform.position) <= distanceToGetAlarm)
        {
            alarmObjectBoid = alarmBoid;
            if (alarmCoroutine != null)
                StopCoroutine(alarmCoroutine);
            alarmCoroutine = StartCoroutine(resetOpositeAlarmDirection());
        }        
    }

    private IEnumerator resetOpositeAlarmDirection()
    {
        yield return new WaitForSeconds(timeForAlarm);
        alarmObjectBoid = null;
        alarmCoroutine = null;
    }

    private void LetsChase(Transform chaseTargetTr)
    {
        chaseTarget = chaseTargetTr;
        foreach (BoxCollider boid in boids)
        {
            if (boid != null && boid.gameObject != gameObject)
                boid.gameObject.GetComponent<Boid>().GetChase(chaseTargetTr);
        }
    }

    public void GetChase(Transform chaseTargetTr)
    {
        chaseTarget = chaseTargetTr;
    }

    
    public float viewRadius;
    public float viewAngle;
    public List<Transform> visibleTargets = new List<Transform>();
    public LayerMask targetMask;
    public LayerMask obstacleMask;    

    //https://github.com/SebLague/Field-of-View/blob/master/Episode%2003/FieldOfView.cs
    void Rule7()
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);                
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    Debug.DrawLine(transform.position, transform.position + dirToTarget * dstToTarget, myColor);
                    visibleTargets.Add(target);
                }
            }
        }
    }

    void Rule8()
    {
        
        foreach (Transform visibleTarget in visibleTargets)
        {
            if (tagsToFearSomeone.Count > 0 && tagsToFearSomeone.Contains(visibleTarget.tag))
            {
                MakeAlarm(visibleTarget.GetComponent<Boid>());
                // Debug.Log("Make alarm boid: " + gameObject.name);
                return;
            }
            else if ( tagsToKillSomeone.Count > 0 && tagsToKillSomeone.Contains(visibleTarget.tag))
            {
                if (chaseTarget == null)
                {
                    LetsChase(visibleTarget.transform);
                    Debug.Log("Boid will chase someone: " + gameObject.name);
                    return;
                }
                else
                {
                    if (chaseCoroutine != null) 
                        StopCoroutine(chaseCoroutine);
                    chaseCoroutine = StartCoroutine(resetChaseTarget());
                }
            }            
        }       
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (tagsToKillSomeone.Count > 0 && tagsToKillSomeone.Contains(collision.gameObject.tag))
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Boid"))
            {
                collision.gameObject.GetComponent<Boid>().Die();
            }
            if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                GameManager.instance.LoseGame();
            }
        }
    }

    private IEnumerator resetChaseTarget()
    {
        yield return new WaitForSeconds(resetTimeForChase);
        chaseTarget = null;
        chaseCoroutine = null;
    }


    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }

    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0)
            {
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }

            }


            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]) + Vector3.forward * maskCutawayDst;

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();

        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }


    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }


    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, obstacleMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }


    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

}