using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace elZach.Common{
    public static class EventsExtension
    {
        public static GameObject GetGameObjectFromPointer(this EventSystem eventSystem, int? pointerId = null)
        {
            var result = eventSystem.GetRaycastResultFromPointer(pointerId);
            if (result == null) return null;
    
            var raycast = result.Value;
    
            var graphic = raycast.gameObject.GetComponentInParent<Graphic>();
            if (graphic) return graphic.gameObject;
    
            var eventHandler = raycast.gameObject.GetComponentInParent<IEventSystemHandler>() as Component;
            if (eventHandler) return eventHandler.gameObject;
    
            return null;
        }

        public static RaycastResult? GetRaycastResultFromPointer(this EventSystem eventSystem, int? pointerId = null)
        {
            if (!eventSystem) return null;
    
            pointerId ??= Mouse.current?.deviceId ?? -1;
    
            var module = eventSystem.currentInputModule as InputSystemUIInputModule;
            if (!module) return null;
    
            var result = module.GetLastRaycastResult(pointerId.Value);
            return result.isValid ? result : null;
        }
    }
}