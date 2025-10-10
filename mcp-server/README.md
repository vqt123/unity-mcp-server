# Unity MCP Server - Python Side

This is the MCP (Model Context Protocol) server that connects AI assistants to Unity.

## Setup

```bash
# Install dependencies
pip3 install -r requirements.txt
```

## Configuration

Add to your Claude Desktop config file:

**macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`

```json
{
  "mcpServers": {
    "unity": {
      "command": "python3",
      "args": ["/FULL/PATH/TO/mcptest/mcp-server/unity_mcp_server.py"]
    }
  }
}
```

Replace `/FULL/PATH/TO/` with the actual path. Get it with:

```bash
cd /Users/vinhtrinh/code/mcptest/mcp-server
pwd
```

## Testing

Make sure Unity is running, then test the server manually:

```bash
# Test ping
curl -X POST http://localhost:8765/ \
  -H "Content-Type: application/json" \
  -d '{"tool":"unity_ping","args":{}}'

# Test get scene info
curl -X POST http://localhost:8765/ \
  -H "Content-Type: application/json" \
  -d '{"tool":"unity_get_scene_info","args":{}}'

# Test create cube
curl -X POST http://localhost:8765/ \
  -H "Content-Type: application/json" \
  -d '{"tool":"unity_create_cube","args":{"name":"TestCube","x":5,"y":2,"z":3}}'
```

## Tools Available

- `unity_ping` - Health check
- `unity_get_scene_info` - Get current scene information
- `unity_create_cube` - Create a cube at specified position

