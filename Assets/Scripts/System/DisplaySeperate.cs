using UnityEngine;

// 멀티 디스플레이를 활성화하는 클래스
// - 기본적으로 Unity는 첫 번째 디스플레이만 활성화함
// - 추가 디스플레이가 있으면 이를 활성화
// scene의 singleton pattern 객체에 추가해서 사용
public class DisplaySeperate : MonoBehaviour
{
    private void Start()
    {
        // 사용 가능한 디스플레이가 2개 이상이면 두 번째 디스플레이 활성화
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
            Debug.Log("[INFO] Second display activated.");
        }

        // 사용 가능한 디스플레이가 3개 이상이면 세 번째 디스플레이 활성화
        if (Display.displays.Length > 2)
        {
            Display.displays[2].Activate();
            Debug.Log("[INFO] Third display activated.");
        }
    }
}