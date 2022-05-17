using UnityEngine;

public class SmokeController : MonoBehaviour
{
	public LineRenderer line;
	private Transform tr;
	private Vector3[] positions;
	private Vector3[] directions;
	private int i;
	private float timeSinceUpdate = 0.0f;
	private Material lineMaterial;
	private float lineSegment = 0.0f;
	private int currentNumberOfPoints = 2;
	private bool allPointsAdded = false;
	private float timeOffset;
	private Vector3 oldPos;
	private float maxDistance = 0.001f;
	
	public int numberOfPoints = 10;
	public float updateSpeed = 0.25f;
	public float riseSpeed = 0.25f;
	public float spread = 0.2f;
	public float curveSampleSpeed = 1f;
	public AnimationCurve xOffset, yOffset, zOffset;
	
	private Vector3 tempVec;
	
    // Start is called before the first frame update
    void Start()
    {
	    timeOffset = Random.Range(0f, 1f);
	    tr = this.transform;

	    lineMaterial = new Material(line.material);

	    xOffset.postWrapMode = WrapMode.PingPong;
	    xOffset.preWrapMode = WrapMode.PingPong;
	    
	    yOffset.postWrapMode = WrapMode.PingPong;
	    yOffset.preWrapMode = WrapMode.PingPong;
	    
	    zOffset.postWrapMode = WrapMode.PingPong;
	    zOffset.preWrapMode = WrapMode.PingPong;

	    lineSegment = 1.0f / numberOfPoints;

	    positions = new Vector3[numberOfPoints];
	    directions = new Vector3[numberOfPoints];

	    line.positionCount = currentNumberOfPoints;

	    for (i = 0; i < currentNumberOfPoints; i++) 
	    {
		    tempVec = GetSmokeVecSample();
		    directions[i] = tempVec;
		    positions[i] = tr.position;
		    line.SetPosition(i, positions[i]);
	    }
    }

    // Update is called once per frame
    void Update()
    {
	    timeSinceUpdate += Time.deltaTime; // Update time.

	    // If it's time to update the line...
	    if (timeSinceUpdate > updateSpeed || maxDistance < Vector3.Distance(tr.position, oldPos)) 
	    {
		    if (timeSinceUpdate > updateSpeed )
		    {
			    timeSinceUpdate -= updateSpeed;
		    }

		    // Add points until the target number is reached.
		    if (!allPointsAdded) 
		    {
			    currentNumberOfPoints++;
			    line.positionCount = currentNumberOfPoints;
			    tempVec = GetSmokeVecSample();
			    directions[0] = tempVec;
			    positions[0] = tr.position;
			    line.SetPosition(0, positions[0]);
		    }

		    if ( !allPointsAdded && (currentNumberOfPoints == numberOfPoints)) 
		    {
			    allPointsAdded = true;
		    }

		    // Make each point in the line take the position and direction of the one before it (effectively removing the last point from the line and adding a new one at transform position).
		    for (i = currentNumberOfPoints - 1; i > 0; i--) 
		    {
			    tempVec = positions[i-1];
			    positions[i] = tempVec;
			    tempVec = directions[i-1];
			    directions[i] = tempVec;
		    }
		    tempVec = GetSmokeVecSample();
		    directions[0] = tempVec; // Remember and give 0th point a direction for when it gets pulled up the chain in the next line update.

		    oldPos = tr.position;
	    }

	    // Update the line...
	    for (i = 1; i < currentNumberOfPoints; i++)
	    {
		    tempVec = positions[i];
		    tempVec += directions[i] * Time.deltaTime;
		    positions[i] = tempVec;

		    line.SetPosition(i, positions[i]);
	    }
	    positions[0] = tr.position; // 0th point is a special case, always follows the transform directly.
	    line.SetPosition(0, tr.position);

	    // If we're at the maximum number of points, tweak the offset so that the last line segment is "invisible" (i.e. off the top of the texture) when it disappears.
	    // Makes the change less jarring and ensures the texture doesn't jump.
	    if (allPointsAdded) 
	    {
		    lineMaterial.SetTextureOffset("_MainTex", new Vector2(lineSegment * ( timeSinceUpdate / updateSpeed ), 0));
	    }
    }
    
    private Vector3 GetSmokeVec()
    {
	    Vector3 smokeVec;
	    smokeVec.x = Random.Range(-1.0f, 1.0f);
	    smokeVec.y = Random.Range(0.0f, 1.0f);
	    smokeVec.z = Random.Range(-1.0f, 1.0f);
	    smokeVec.Normalize ();
	    smokeVec *= spread;
	    smokeVec.y += riseSpeed;
	    return smokeVec;
    }
    
    private Vector3 GetSmokeVecSample()
    {
	    Vector3 smokeVec;

	    smokeVec.x = xOffset.Evaluate((Time.time  + timeOffset) * curveSampleSpeed);
	    smokeVec.y = yOffset.Evaluate((Time.time  + timeOffset) * curveSampleSpeed);
	    smokeVec.z = zOffset.Evaluate((Time.time  + timeOffset) * curveSampleSpeed);
	    
	    smokeVec.Normalize();
	    smokeVec *= spread;
	    smokeVec.y += riseSpeed;
	    return smokeVec;
    }
    
}
