using System.Reflection;
using Godot;

namespace RPG.scripts.combat;

public partial class AttributeSystem : Resource {
    
    public Attributes Total = new();
    

    public void Link(Attributes pOtherAttributes) {
        if (pOtherAttributes.IsConnectedToValueChanged(OnAttributeChanged)) {
            return;
        }

        foreach (string propertyName in pOtherAttributes.GetPropertyNames()) {
            Total.Set(propertyName, pOtherAttributes.Get(propertyName).As<long>());
        }
        
        pOtherAttributes.ValueChanged += OnAttributeChanged;
    }

    public void Unlink(Attributes pOtherAttributes) {
        if (!pOtherAttributes.IsConnectedToValueChanged(OnAttributeChanged)) {
            return;
        }
        
        foreach (string propertyName in pOtherAttributes.GetPropertyNames()) {
            Total.Set(propertyName, -pOtherAttributes.Get(propertyName).As<long>());
        }
        
        pOtherAttributes.ValueChanged -= OnAttributeChanged;
    }

    private void OnAttributeChanged(string pPropertyName, long pDelta) {
        Total.Set(pPropertyName, pDelta);
    }

}