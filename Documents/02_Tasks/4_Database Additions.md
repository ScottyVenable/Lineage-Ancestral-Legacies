# Database Additions

## Items & Equipment
- Assign a slots enum to an entity and the number of items that can be equipt on a slot (2 rings, etc.)
- Figure out how we can handle slots on entities who don't have a certain slot (Wolves cant wear rings, etc.)


## Refactor
- Adjust code to comply with the GameDatabase files, and all files should be able to work together. Any other logic that uses a Database but not the most recent usage from the GameDatabase folder needs to be changed to the most recent and spread universally to avoid errors.
- Ensure struct references are updated to the new system and use the right syntax for their references. Adjust any outdated or non complient code.
- Adjust all Data types to follow a struct base structure (ID, Name, Description, Type, etc.) and then add any nessisary fields to the structs to make them reflect their intended purpose.
- Implement a system for managing item slots and equipable items, ensuring compatibility with the new struct-based approach.
- Update all outdated code that doesnt use the new gamedatabase system.