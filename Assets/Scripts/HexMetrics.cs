﻿using UnityEngine;

public static class HexMetrics {

	public const float OuterRadius = 10f;
	public const float İnnerRadius = OuterRadius * 0.866025404f;

	public static Vector3[] corners = {
		new Vector3(0f, 0f, OuterRadius),
		new Vector3(İnnerRadius, 0f, 0.5f * OuterRadius),
		new Vector3(İnnerRadius, 0f, -0.5f * OuterRadius),
		new Vector3(0f, 0f, -OuterRadius),
		new Vector3(-İnnerRadius, 0f, -0.5f * OuterRadius),
		new Vector3(-İnnerRadius, 0f, 0.5f * OuterRadius),
		new Vector3(0f, 0f, OuterRadius)
	};
}