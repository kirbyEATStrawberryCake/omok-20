using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 싱글톤 베이스 클래스
/// </summary>
/// <typeparam name="T">싱글톤으로 만들 컴포넌트 타입</typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static bool isShuttingDown = false;


    public static T Instance
    {
        get
        {
            if (isShuttingDown)
            {
                Debug.LogWarning($"[Singleton] '{typeof(T)}' 인스턴스는 이미 파괴되었습니다. null을 반환합니다.");
                return null;
            }

            if (instance == null)
            {
                instance = FindFirstObjectByType<T>();
                if (instance == null)
                {
                    GameObject singletonObject = new($"{typeof(T).Name} (Singleton)");
                    instance = singletonObject.AddComponent<T>();
                }
            }

            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;

            // 씬 전환시에 호출되는 액션 메서드 할당 
            SceneManager.sceneLoaded += OnSceneLoad;
        }
        else if (instance != this)
        {
            Debug.Log($"[Singleton] {typeof(T).Name}의 중복 인스턴스가 감지되어 파괴");
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
            instance = null;
        }
    }

    protected virtual void OnApplicationQuit()
    {
        isShuttingDown = true;
    }

    protected abstract void OnSceneLoad(Scene scene, LoadSceneMode mode);
}