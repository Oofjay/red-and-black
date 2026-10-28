using UnityEngine.EventSystems;

namespace AdvancedColorPicker
{
    public class WindowOrderer : UIBehaviour, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            transform.SetAsLastSibling();
        }
    }
}