using UnityEngine;

[RequireComponent(typeof(Camera))]
public class RipplePostProcessor : MonoBehaviour
{
    /// <summary>
    /// Singleton reference to access RipplePostProcessor anywhere.
    /// You can change this to any other kind of reference that works for your project.
    /// </summary>
    public static RipplePostProcessor ins;


    private const float EXPECTED_DELTATIME_AT_60FPS = 1f / 60f;
    public const float LOWEST_AMOUNT_VALUE = 0.0001f;
    public Material RippleMaterial;
    public float MaxAmount = 50f;

    [Range(0, 1)]
    public float Friction = .9f;

    public float CurrentAmount = 0f;
    private bool _update = false;

    public bool DEBUG_ToggleRipple;

    private void Awake()
    {
        if (ins != null)
        {
            Destroy(this);
            return;
        }
        ins = this;
    }
    private void Start()
    {
        CurrentAmount = RippleMaterial.GetFloat("_Amount");
        _update = CurrentAmount > LOWEST_AMOUNT_VALUE;
    }
    public void Ripple()
    {
        CurrentAmount = MaxAmount;
        Vector2 pos = new Vector2(Screen.width, Screen.height) / 2f;
        RippleMaterial.SetFloat("_CenterX", pos.x);
        RippleMaterial.SetFloat("_CenterY", pos.y);
        _update = true;
    }
    void Update()
    {
        if (DEBUG_ToggleRipple)
        {
            DEBUG_ToggleRipple = false;
            Ripple();
        }
        if (_update)
        {
            RippleMaterial.SetFloat("_Amount", CurrentAmount);
            float amountToReduce = CurrentAmount * (1 - Friction);
            CurrentAmount -= amountToReduce * (Time.deltaTime / EXPECTED_DELTATIME_AT_60FPS);
            if (CurrentAmount < LOWEST_AMOUNT_VALUE)
            {
                _update = false;
                CurrentAmount = 0;
                RippleMaterial.SetFloat("_Amount", CurrentAmount);
            }
        }
    }
    /// <summary>
    /// Only works if you dont have Universal/HD Render Pipline installed.
    /// Delete or comment this out if you dont have URP or HDRP installed.
    /// </summary>
    //void OnRenderImage(RenderTexture src, RenderTexture dst)
    //{
    //    Graphics.Blit(src, dst, RippleMaterial);
    //}

    private void OnApplicationQuit()
    {
        RippleMaterial.SetFloat("_Amount", 0);
        RippleMaterial.SetFloat("_CenterX", 0);
        RippleMaterial.SetFloat("_CenterY", 0);
    }
}