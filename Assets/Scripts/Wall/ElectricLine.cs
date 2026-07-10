using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ElectricLine : MonoBehaviour
{
    private LineRenderer mainLineRenderer;
    private LineRenderer[] subLineRenderers; // 追加のサブ線

    public int segments = 15; // ⚡の折れ曲がりを複雑にするため少し増やす
    public float jitterAmount = 0.4f; // ⚡のトゲトゲを少し大きく
    public float updateSpeed = 0.02f; // 更新速度を上げてより激しく動かす
    public int subLineCount = 2; // サブの線を2本追加

    [Header("💡 電気のベースカラー")]
    public Color electricColor = new Color(1f, 0.85f, 0f); // 💡 インスペクターから好きな黄色を選べます！

    [Header("💡 光の強さ（ここを上げても真っ白になりにくくしました）")]
    public float glowIntensity = 3f;

    private Vector3 startPos;
    private Vector3 endPos;
    private float timer;

    void Start()
    {
        // メインのLine Renderer
        mainLineRenderer = GetComponent<LineRenderer>();
        mainLineRenderer.useWorldSpace = false; // 壁に固定

        // インスペクターで設定した始点と終点を記憶
        startPos = mainLineRenderer.GetPosition(0);
        endPos = mainLineRenderer.GetPosition(mainLineRenderer.positionCount - 1);
        mainLineRenderer.positionCount = segments;

        // 強制的にHDRカラーを設定
        ApplyHdrColor(mainLineRenderer);

        // サブの線を作成
        subLineRenderers = new LineRenderer[subLineCount];
        for (int i = 0; i < subLineCount; i++)
        {
            GameObject subLineObject = new GameObject($"SubLine_{i}");
            subLineObject.transform.SetParent(this.transform);
            subLineObject.transform.localPosition = Vector3.zero;
            subLineObject.transform.localRotation = Quaternion.identity;

            LineRenderer lr = subLineObject.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.positionCount = segments;
            lr.material = mainLineRenderer.material; // メインと同じマテリアルを使用
            lr.widthMultiplier = mainLineRenderer.widthMultiplier * 0.5f; // メインより細く

            // サブの線も発光させる
            ApplyHdrColor(lr);

            subLineRenderers[i] = lr;
        }
    }

    void ApplyHdrColor(LineRenderer lr)
    {
        // 💡 2の冪乗（パワー）を使ってUnityのHDR内部システムに「この色を光らせて！」と正しく命令します
        //// これにより、色が白く潰れずに「光る黄色」を維持できるようになります
        //float factor = Mathf.Pow(2f, glowIntensity);
        //Color hdrColor = new Color(electricColor.r * factor, electricColor.g * factor, electricColor.b * factor, electricColor.a);

        //lr.startColor = hdrColor;
        //lr.endColor = hdrColor;
        //lr.colorGradient = new Gradient(); // グラデーション無効化
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateSpeed)
        {
            UpdateAllLines();
            timer = 0;
        }
    }

    void UpdateAllLines()
    {
        UpdateSingleLine(mainLineRenderer, startPos, endPos, jitterAmount);

        for (int i = 0; i < subLineCount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-jitterAmount * 0.5f, jitterAmount * 0.5f),
                Random.Range(-jitterAmount * 0.5f, jitterAmount * 0.5f),
                Random.Range(-jitterAmount * 0.5f, jitterAmount * 0.5f)
            );

            UpdateSingleLine(subLineRenderers[i], startPos + offset, endPos + offset, jitterAmount * 0.7f);
        }
    }

    void UpdateSingleLine(LineRenderer lr, Vector3 start, Vector3 end, float jitter)
    {
        lr.SetPosition(0, start);

        for (int i = 1; i < segments - 1; i++)
        {
            float t = (float)i / (segments - 1);
            Vector3 targetPos = Vector3.Lerp(start, end, t);

            Vector3 noise = new Vector3(
                Random.Range(-jitter, jitter),
                Random.Range(-jitter, jitter),
                Random.Range(-jitter, jitter)
            );

            lr.SetPosition(i, targetPos + noise);
        }

        lr.SetPosition(segments - 1, end);
    }
}