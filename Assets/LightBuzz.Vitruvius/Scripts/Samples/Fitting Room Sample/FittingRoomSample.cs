using UnityEngine;
using UnityEngine.UI;
using LightBuzz.Vitruvius;
using LightBuzz.Vitruvius.Avateering;
using Windows.Kinect;
using LightBuzz.Vitruvius.Gestures;
using System.Collections;

public class FittingRoomSample : VitruviusSample
{
    #region Variables

    public Text text_userSize;

    GestureController gestureController;

    bool isCounting = false;
    enum standingState {
        undefined,
        forward,
        backward,
    }

    float bodyHeightSum=0;
    float shoulderDistSum=0;
    float upperHeightSum = 0;

    int timeCount = 5;
    int totalCount = 0;

    private const float INITIAL_DISTANCE = 3f;
    public float SCALE_MULTIPLIER = 0.9f;

    standingState curState = standingState.undefined;
    
    public Transform currShirtSpine;
    /*
     * Calculated by pausing unity frame and adapting manually the shirt oin users body 
     */

    //depends on users body
    const float X_OFFSET = -0.14f;
    const float Y_OFFSET = +0.2f;

    public const float INITIAL_SCALE_FACTOR = 0.395f;
    public float FINAL_SCALE_FACTOR;

    BodyWrapper body;
    bool onlyOnce;

    public bool useGreenScreen = true;
    public GameObject greenScreenView;

    public AvatarCloth[] clothes = new AvatarCloth[5];

    //[HideInInspector]
    public bool[] selected;
    public Vector2[] buttonPositions = new Vector2[5];
    public Vector2 buttonSize;

    public SkinnedMeshRenderer shirtSkinnedMeshRenderer;
    public Texture[] shirtTextures;

    //Face pART
    Face face;

    int currIndex;
    #endregion

    #region Reserved methods // Awake - OnApplicationQuit - Update - OnGUI

    protected override void Awake()
    {
        base.Awake();

        Avateering.Enable();

        for (int i = 0; i < clothes.Length; i++)
        {
            clothes[i].Initialize();
        }

        selected = new bool[clothes.Length];

        curState = standingState.forward;
        onlyOnce = true;
        currIndex = 0;
        FINAL_SCALE_FACTOR = 0;
        SCALE_MULTIPLIER = 0.9f;
        text_userSize.text = "";

        for (int i = 0; i < clothes.Length; i++)
        {
            selected[i] = !selected[i];

            if (!selected[i] && clothes[i])
            {
                clothes[i].Reset();
            }
        }

        gestureController = new GestureController();
        gestureController.GestureRecognized += GestureRecognized;
        gestureController.Start();

    }
    

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();

        Avateering.Disable();
        if (gestureController != null)
        {
            gestureController.Stop();
            gestureController.GestureRecognized -= GestureRecognized;
            gestureController = null;
        }

        for (int i = 0; i < clothes.Length; i++)
        {
            clothes[i].Dispose();
        }
    }

    void Update()
    {
        if (useGreenScreen)
        {
            if (colorFrameReader != null && depthFrameReader != null && bodyIndexFrameReader != null)
            {
                using (ColorFrame colorFrame = colorFrameReader.AcquireLatestFrame())
                using (DepthFrame depthFrame = depthFrameReader.AcquireLatestFrame())
                using (BodyIndexFrame bodyIndexFrame = bodyIndexFrameReader.AcquireLatestFrame())
                {
                    if (colorFrame != null && depthFrame != null && bodyIndexFrame != null)
                    {
                        frameView.FrameTexture = colorFrame.GreenScreen(depthFrame, bodyIndexFrame);
                    }
                }
            }
        }
        else
        {
            switch (visualization)
            {
                case Visualization.Color:
                    UpdateColorFrame();
                    break;
                case Visualization.Depth:
                    UpdateDepthFrame();
                    break;
                default:
                    UpdateInfraredFrame();
                    break;
            }
        }

        UpdateBodyFrame();

        if (useGreenScreen)
        {
            if (!greenScreenView.activeSelf)
            {
                greenScreenView.SetActive(true);
            }
        }
        else if (greenScreenView.activeSelf)
        {
            greenScreenView.SetActive(false);
        }

        if (body != null)
        {
            gestureController.Update(body);

            //check if face is detected
            if (faceFrameReader != null)
            {
                if (!faceFrameSource.IsTrackingIdValid)
                {
                    faceFrameSource.TrackingId = body.TrackingId;
                }

                using (var faceFrame = faceFrameReader.AcquireLatestFrame())
                {
                    if (faceFrame != null)
                    {
                        face = faceFrame.Face();
                    }
                }
            }

            Vector2 l = body.Map2D[JointType.ShoulderLeft].ToVector2();
            Vector2 r = body.Map2D[JointType.ShoulderRight].ToVector2();

            float shoulderDist = Vector2.Distance(l, r);
                        
            for (int i = 0; i < selected.Length; i++)
            {
                if (selected[i])
                {
                    AvatarCloth cloth = clothes[i];
                    Avateering.Update(cloth, body);
                    /*

                    if (shoulderDist < 190) {
                        if (face != null) {
                            if (face.Quality == Microsoft.Kinect.Face.FaceAlignmentQuality.High && face.IsTracked && curState == standingState.backward)
                            {
                                if (currShirtSpine.eulerAngles.y > 225)
                                {
                                    // probably turning left area
                                    curState = standingState.forward;
                                }
                                else if (currShirtSpine.eulerAngles.y < 135)
                                {
                                    // probably turning right area
                                    curState = standingState.forward;
                                }

                            }
                            else if(!face.IsTracked && curState == standingState.forward) {
                                if (currShirtSpine.eulerAngles.y > 225)
                                {
                                    // probably turning left area
                                    curState = standingState.backward;

                                }
                                else if (currShirtSpine.eulerAngles.y < 135)
                                {
                                    // probably turning right area
                                    curState = standingState.backward;

                                }
                            }
                        }
                    }

                    //Debug.Log(" cur status ==" + curState.ToString() +" ;;;  shoulder dis == " +shoulderDist.ToString("0.00") +" ;;;  cur rotation ==  "+ currShirtYRotation);

                    if (curState == standingState.backward)
                    {
                        currShirtSpine.parent.parent.eulerAngles = Vector3.zero;
                        cloth.mirroredModel = false;
                    }
                    else {
                        currShirtSpine.parent.parent.eulerAngles = Vector3.up * 180;
                        cloth.mirroredModel = true ;
                    }
                    */
                    if (isCounting) {

                        bodyHeightSum += (float)body.Height();
                        upperHeightSum += (float)body.UpperHeight() * 100;
                        shoulderDistSum += shoulderDist;

                        totalCount++;
                    }

                    Vector2 position = body.Joints[cloth.Pivot].Position.ToPoint(useGreenScreen ? Visualization.Depth : visualization, CoordinateMapper);
                    float bodyZ = body.Joints[cloth.Pivot].Position.Z;
                    if (!float.IsInfinity(position.x) && !float.IsInfinity(position.y))
                    {
                        position = frameView.GetPositionOnFrame(position);

                        if (position == null)
                            Debug.Log("cloth is null");
                        cloth.SetBonePosition(cloth.Pivot, new Vector3(position.x-X_OFFSET, position.y+Y_OFFSET));
                        
                        float distance = cloth.JointInfos[(int)cloth.Pivot].RawPosition.z;
                        if (distance != 0)
                        {
                          cloth.body.transform.localScale = Rescale(cloth,distance);
                        }
                    }
                }
            }
        }
        else
        {
            resetClothes();
        }
    }

    Vector3 Rescale(AvatarCloth cloth,float dis) {
        
        Vector3 newScale = Vector3.zero;

        newScale = SCALE_MULTIPLIER*(cloth.ScaleOrigin*FINAL_SCALE_FACTOR* frameView.ViewScale)*(AvatarCloth.colorScaleFactor / dis);

        return newScale;
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10f,10f,100,50), "Re - Fit"))
        {
            if(!isCounting)
                StartCoroutine(Counting());
        }
    }

    IEnumerator Counting() {
        
        isCounting = true;
        yield return new WaitForSeconds(timeCount);
        isCounting = false;
        float finalHeight = bodyHeightSum / totalCount;
        float finalShoulderDist = shoulderDistSum / totalCount;
        float finalUpperHeight = upperHeightSum / totalCount;

        Vector2 l = body.Map2D[JointType.ShoulderLeft].ToVector2();
        Vector2 r = body.Map2D[JointType.ShoulderRight].ToVector2();

        UserMeasurement.width = finalShoulderDist;
        UserMeasurement.lenght = finalUpperHeight;
        UserMeasurement.height = finalHeight;

        FINAL_SCALE_FACTOR = INITIAL_SCALE_FACTOR * finalHeight;

        Debug.Log("Height: " + finalHeight + "  Lenght: " + finalUpperHeight + "  Width: " + UserMeasurement.width + "Scale factor: " + FINAL_SCALE_FACTOR);
        
        text_userSize.text = UserMeasurement.getUserClothSize();

        bodyHeightSum = 0;
        upperHeightSum = 0;
        shoulderDistSum = 0;

        totalCount = 0;
        finalHeight = 0;
    }
    
    private void resetClothes()
    {
        for (int i = 0; i < clothes.Length; i++)
        {
            if(clothes[i])
                clothes[i].Reset();
        }
    }
    
    #endregion
    public void ChangeShirtColor() {

        if (currIndex+1 >= shirtTextures.Length)
            currIndex = 0;
        else
            currIndex++;

        shirtSkinnedMeshRenderer.material.mainTexture = shirtTextures[currIndex];

    }


    #region OnBodyFrameReceived
    protected override void OnBodyFrameReceived(BodyFrame frame)
    {
        Body body = frame.Bodies().Closest();

        if (body != null)
        {
            if (this.body == null)
            {
                this.body = BodyWrapper.Create(body, CoordinateMapper, useGreenScreen ? Visualization.Depth : visualization);
                OnBodyEnter();
            }
            else
            {
                //this.body.RotationAngle();
               this.body.Set(body, CoordinateMapper, useGreenScreen ? Visualization.Depth : visualization);
                //Debug.Log("reset ????");
            }
                        
        }
        else if (this.body != null)
        {
            this.body = null;
            OnBodyExit();
        }
    }

    #endregion

    #region GestureRecognized

    void GestureRecognized(object sender, GestureEventArgs e)
    {
        if (e.GestureType == GestureType.WaveRight)
        {
            if (!isCounting)
                StartCoroutine(Counting());
        }
        else if(e.GestureType == GestureType.SwipeLeft || e.GestureType == GestureType.SwipeRight) 
            ChangeShirtColor();

        Debug.Log("Gesture: <b>" + e.GestureType.ToString() + "</b>");
    }

    #endregion

    #region OnBodyEnter

    void OnBodyEnter()
    {
        if (!isCounting)
            StartCoroutine(Counting());
    }

    #endregion

    #region OnBodyExit

    void OnBodyExit()
    {
        resetClothes();
    }

    #endregion
}