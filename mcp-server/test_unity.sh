#!/bin/bash

# Test script for Unity MCP Server

echo "Testing Unity MCP Server..."
echo ""

echo "1. Testing ping..."
curl -X POST http://localhost:8765/ \
  -H "Content-Type: application/json" \
  -d '{"tool":"unity_ping","args":{}}'
echo ""
echo ""

echo "2. Testing get_scene_info..."
curl -X POST http://localhost:8765/ \
  -H "Content-Type: application/json" \
  -d '{"tool":"unity_get_scene_info","args":{}}'
echo ""
echo ""

echo "3. Testing create_cube..."
curl -X POST http://localhost:8765/ \
  -H "Content-Type: application/json" \
  -d '{"tool":"unity_create_cube","args":{"name":"TestCube","x":5,"y":2,"z":3}}'
echo ""
echo ""

echo "✅ Tests complete! Check Unity Editor for the cube."

