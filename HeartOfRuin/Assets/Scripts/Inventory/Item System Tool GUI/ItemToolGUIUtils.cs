using System;
using System.IO;
using System.Text;
using UnityEngine;
using WebSocketSharp;

static class ItemIDGenerator
{
    public static string GenerateID()
    {
        return Guid.NewGuid().ToString("N"); // stable, compact
    }
}

// Sanitize filename by removing invalid chars and trimming.

