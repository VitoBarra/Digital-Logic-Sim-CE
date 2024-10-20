using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;

public class ChipPackageDisplay : MonoBehaviour
{
    public TMPro.TextMeshPro nameText;
    public Transform container;
    private SpawnableChip Chip;
    private MeshRenderer MeshRenderer;
    public List<Transform> PalteObjects;


    void Awake()
    {
        ScalingManager.i.OnScaleChange += RescaleChipPackage;
        Init();
    }

    private void OnDestroy()
    {
        ScalingManager.i.OnScaleChange -= RescaleChipPackage;
    }

    public void Init()
    {
        Chip = GetComponent<SpawnableChip>();
        MeshRenderer = container.GetComponent<MeshRenderer>();
        DrawPackageChip();
        SetColour(Chip.PackageGraphicData.PackageColour);
    }

    private void SetColour(Color dataColour)
    {
        if (MeshRenderer is null) return;
        MeshRenderer.material.color = dataColour;
    }

    public void SetUpForCustomPackageChip(ChipInfo info)
    {
        gameObject.name = info.name;
        nameText.text = info.name;
        nameText.color = info.PackNameColor;
        SetColour(info.PackColor);
    }


    private void RescaleChipPackage()
    {
        DrawPackageChip();
    }


    private void DrawPackageChip()
    {
        if (Chip == null) return;
        nameText.fontSize = ScalingManager.PackageFontSize;

        var graphicalData = Chip.PackageGraphicData;

        float PinRadius = PinDisplay.radius * 0.25f;
        float PinInteraction = PinRadius * PinDisplay.IteractionFactor;
        float pinSpacePadding = PinRadius * 0.2f;

        int numPins = Mathf.Max(Chip.inputPins.Count, Chip.outputPins.Count);

        float containerWidth = 0;
        float containerHeight = 0;

        if (graphicalData.OverrideWidthAndHeight)
        {
            containerWidth = graphicalData.Width;
            containerHeight = graphicalData.Height;
        }
        else
        {
            containerWidth = nameText.preferredWidth + PinInteraction * 2f;
            containerHeight = Mathf.Max(numPins * (PinRadius * 2 + pinSpacePadding), nameText.preferredHeight + 0.05f) ;
        }

        float topPinY = containerHeight / 2 - PinRadius;
        float bottomPinY = -containerHeight / 2 + PinRadius;
        const float z = -0.05f;

        //Applay padding
        containerWidth += graphicalData.WidthPadding*Mathf.Lerp(0.2f,1,ScalingManager.Scale) ;
        containerHeight += graphicalData.HeightPadding*Mathf.Lerp(0.2f,1,ScalingManager.Scale);


        // Input pins
        int numInputPinsToAutoPlace = Chip.inputPins.Count;
        for (int i = 0; i < numInputPinsToAutoPlace; i++)
        {
            float percent = 0.5f;
            if (Chip.inputPins.Count > 1)
            {
                percent = i / (numInputPinsToAutoPlace - 1f);
            }


            float posX = -containerWidth / 2f;
            float posY = Mathf.Lerp(topPinY, bottomPinY, percent);
            Chip.inputPins[i].transform.localPosition = new Vector3(posX, posY, z);

        }

        // Output pins
        for (int i = 0; i < Chip.outputPins.Count; i++)
        {
            float percent = 0.5f;
            if (Chip.outputPins.Count > 1)
            {
                percent = i / (Chip.outputPins.Count - 1f);
            }

            float posX = containerWidth / 2f;
            float posY = Mathf.Lerp(topPinY, bottomPinY, percent);
            Chip.outputPins[i].transform.localPosition = new Vector3(posX, posY, z);
        }

        // Set container size
        container.localScale = new Vector3(containerWidth, containerHeight, 1);
        GetComponent<BoxCollider2D>().size = new Vector2(containerWidth, containerHeight);



        // Scale Plate Objects
        if ( PalteObjects is null || PalteObjects.Count == 0) return;

        foreach (var obj in PalteObjects)
            obj.localScale = new Vector3(containerWidth- (containerWidth / 100 * 15), containerHeight-(containerHeight / 100 * 15), 1);



    }

    private void OnValidate()
    {
        if (ScalingManager.i is null) return;
        RescaleChipPackage();
    }



}