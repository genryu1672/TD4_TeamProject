using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("生成するプレイヤーのプレファブ")]
    public GameObject playerPrefab;
    public Vector3 playerStartPosition = new Vector3(0, 1, 0);

    [Header("生成する壁（Wall）のプレファブ")]
    public GameObject wallPrefab;
    [Header("プレイヤーからどれくらい後ろに壁を出すか")]
    public float wallOffsetZ = -5f;

    [Header("関連するスクリプト")]
    public MapGenerator mapGenerator;
    public SimpleCameraFollow cameraFollow;

    void Awake()
    {
        // 1. プレイヤーを生成
        GameObject player = Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
        Transform playerTransform = player.transform;

        // 2. 壁（Wall）をプレイヤーの真後ろに生成
        if (wallPrefab != null)
        {
            Vector3 wallStartPosition = playerStartPosition + new Vector3(0, 0, wallOffsetZ);
            Instantiate(wallPrefab, wallStartPosition, Quaternion.identity);
        }

        // 3. マップ生成スクリプトにプレイヤーを教える
        if (mapGenerator != null)
        {
            mapGenerator.SetPlayer(playerTransform);
        }

        // 4. カメラ追従スクリプトにプレイヤーを教える
        if (cameraFollow != null)
        {
            cameraFollow.target = playerTransform;
        }
    }
}