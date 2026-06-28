using System;
using UnityEngine;
using Molca.ColorID;

namespace MolcaSDK.Utilities
{
    [Serializable]
    public struct BooleanColor
    {
        public ColorIDReference trueColor;
        public ColorIDReference falseColor;

        public BooleanColor(string trueColorId = "Success", string falseColorId = "Error")
        {
            this.trueColor = new ColorIDReference(trueColorId);
            this.falseColor = new ColorIDReference(falseColorId);
        }

        public Color GetColor(bool value)
        {
            return value ? trueColor.Color : falseColor.Color;
        }

        public Color GetColorWithAlpha(bool value, float alpha)
        {
            return value ? trueColor.GetColorWithAlpha(alpha) : falseColor.GetColorWithAlpha(alpha);
        }

        public string GetColorId(bool value)
        {
            return value ? trueColor.ColorId : falseColor.ColorId;
        }

        public void SetTrueColor(string colorId)
        {
            trueColor.ColorId = colorId;
        }

        public void SetFalseColor(string colorId)
        {
            falseColor.ColorId = colorId;
        }

        public bool IsValid()
        {
            return trueColor.IsValid() && falseColor.IsValid();
        }
    }
}