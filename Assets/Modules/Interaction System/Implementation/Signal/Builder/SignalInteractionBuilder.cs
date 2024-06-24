using System;
using Interaction.Signal;
using Interaction.Signal.Display;
using UnityEngine;

namespace Interaction.Signal
{
    public class SignalInteractionBuilder
    {
        SignalInteraction SignalInteractablePref;
        Transform SignalHolder;
        event Action<Chip> OnDeleteChip;
        float BoundsBottom;
        float BoundsTop;
        private float xContainer;
        private float zContainer;
        
        int NextGroupID = 0;
        EditorInterfaceType editorInterfaceType;
        
        public SignalInteractionBuilder(SignalInteraction signalInteractablePref, Transform signalHolder, Action<Chip> onDeleteChip,float boundsBottom,float boundsTop,float _xContainer,float _zContainer, EditorInterfaceType _editorInterfaceType)
        {
            SignalInteractablePref = signalInteractablePref;
            SignalHolder = signalHolder;
            OnDeleteChip = onDeleteChip;

            BoundsBottom = boundsBottom;
            BoundsTop = boundsTop;
            editorInterfaceType = _editorInterfaceType;
            xContainer = _xContainer;
            zContainer = _zContainer;
        }


  

        public (int id,SignalInteraction obj) Build(float  yPos, int desiredGroupSize,Pin.WireType wireType = Pin.WireType.Simple
            ,bool requiredFocus= true, bool displayEnabled = true)
        {
            var ContaierPosition = new Vector3(xContainer, yPos, zContainer);
            var SignalInteractable = GameObject.Instantiate(SignalInteractablePref,SignalHolder);
            SignalInteractable.transform.SetPositionAndRotation(ContaierPosition, SignalInteractable.transform.rotation);
            SignalInteractable.Init(wireType, NextGroupID ,BoundsBottom,BoundsTop,editorInterfaceType,ContaierPosition,displayEnabled);
            SignalInteractable.SetUpCreation(OnDeleteChip, desiredGroupSize, requiredFocus);


            NextGroupID++;
            return (NextGroupID-1,SignalInteractable);
        }
    }
}