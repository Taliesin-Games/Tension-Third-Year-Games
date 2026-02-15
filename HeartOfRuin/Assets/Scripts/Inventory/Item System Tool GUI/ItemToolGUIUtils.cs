using System;
using System.IO;
using System.Text;
using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEngine;
using WebSocketSharp;

// simple id generator for items, using GUIDs for uniqueness and compactness. Non sequential.
static class ItemIDGenerator
{
	public static string GenerateID()
	{
		return Guid.NewGuid().ToString("N"); // stable, compact
	}
}

