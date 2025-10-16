"""Unity MCP Tools - Organized by category"""

from .core_tools import CORE_TOOLS
from .scene_tools import SCENE_TOOLS
from .ui_tools import UI_TOOLS
from .gameobject_tools import GAMEOBJECT_TOOLS
from .prefab_tools import PREFAB_TOOLS
from .script_tools import SCRIPT_TOOLS

# Combine all tools
ALL_TOOLS = (
    CORE_TOOLS +
    SCENE_TOOLS +
    UI_TOOLS +
    GAMEOBJECT_TOOLS +
    PREFAB_TOOLS +
    SCRIPT_TOOLS
)

__all__ = ['ALL_TOOLS']

