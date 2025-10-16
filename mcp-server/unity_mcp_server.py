#!/usr/bin/env python3.11
"""
Unity MCP Server - Modular Architecture
Provides comprehensive tools to control Unity Editor via MCP
"""

import asyncio
import httpx
import json
from typing import Any, Dict, List
from mcp.server import Server
from mcp.types import Tool, TextContent

# Import organized tools
from tools import ALL_TOOLS

# Configuration
UNITY_URL = "http://localhost:8765"
TIMEOUT = 30.0

# Create MCP server
app = Server("unity-mcp")

# Use modular tools
TOOLS = ALL_TOOLS

@app.list_tools()
async def list_tools() -> List[Tool]:
    """List available Unity tools"""
    return [Tool(**tool) for tool in TOOLS]

@app.call_tool()
async def call_tool(name: str, arguments: Dict[str, Any]) -> List[TextContent]:
    """Execute a Unity tool"""
    try:
        # Send request to Unity
        async with httpx.AsyncClient(timeout=TIMEOUT) as client:
            response = await client.post(
                UNITY_URL,
                json={
                    "tool": name,
                    "args": arguments
                }
            )
            
            result = response.json()
            
            # Format response
            if result.get("success"):
                return [TextContent(
                    type="text",
                    text=json.dumps(result, indent=2)
                )]
            else:
                error_msg = result.get("error", "Unknown error")
                return [TextContent(
                    type="text",
                    text=f"Error: {error_msg}"
                )]
                
    except httpx.TimeoutException:
        return [TextContent(
            type="text",
            text="Error: Request to Unity timed out. Is Unity Editor running?"
        )]
    except httpx.ConnectError:
        return [TextContent(
            type="text",
            text="Error: Cannot connect to Unity. Make sure Unity Editor is running and MCP Server is started."
        )]
    except Exception as e:
        return [TextContent(
            type="text",
            text=f"Error: {str(e)}"
        )]

async def main():
    """Run the MCP server"""
    from mcp.server.stdio import stdio_server
    
    async with stdio_server() as (read_stream, write_stream):
        await app.run(
            read_stream,
            write_stream,
            app.create_initialization_options()
        )

if __name__ == "__main__":
    asyncio.run(main())
