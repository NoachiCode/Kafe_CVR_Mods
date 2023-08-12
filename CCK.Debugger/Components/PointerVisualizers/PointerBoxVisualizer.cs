﻿using UnityEngine;

namespace Kafe.CCK.Debugger.Components.PointerVisualizers;

public class PointerBoxVisualizer : PointerVisualizer {

    protected internal BoxCollider PointerCollider { private get; set; }

    protected void Start() {
        VisualizerGo.transform.localScale = Vector3.zero;
    }

    private void Update() {
        // Update the size and position to match the pointer
        VisualizerGo.transform.localScale = PointerCollider.size;
        VisualizerGo.transform.localPosition = PointerCollider.center;
    }
}
