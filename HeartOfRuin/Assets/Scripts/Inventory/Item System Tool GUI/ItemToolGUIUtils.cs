using System;

// simple id generator for items, using GUIDs for uniqueness and compactness. Non sequential.
static class ItemIDGenerator
{
	public static string GenerateID()
	{
		return Guid.NewGuid().ToString("N"); // stable, compact
	}
}

