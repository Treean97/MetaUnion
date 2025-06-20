public interface IFocusable
{
    // UI 표시에 필요한 데이터를 담아서 보냄
    void OnFocus();

    // 포커스 해제 시 호출
    void OnDefocus();

    ObjectInfo GetObjectInfo();
}