# Claude Code Guidelines

## Project Overview
3D third-person RPG built in Godot 4.5 with C#.

## AI Design Docs

Design documentation lives in `docs/ai/`.

### Structure
```
docs/ai/
├── backlog.md          # What to work on next
├── systems/            # Current implementation (source of truth)
├── proposals/          # Planned features and changes
└── archive/            # Completed/rejected proposals
```

### Key Conventions

**Systems** (`docs/ai/systems/`)
- One behavior/mechanic per file
- Living docs reflecting current implementation
- Update when implementation changes

**Proposals** (`docs/ai/proposals/`)
- Naming: `YY-MM-DD-short-description.md` (e.g., `26-01-17-melee-attack.md`)
- Include frontmatter with status, modifies, priority, author, created
- Track progress with implementation checklist in the proposal itself

**Statuses**
- `draft` - Not yet designed
- `review` - Design complete, awaiting approval
- `approved` - Ready for implementation
- `in-progress` - Currently being implemented
- `merged` - Implemented, moved to archive
- `rejected` - Not proceeding, moved to archive

**Workflow**
1. New feature → Create proposal in `proposals/<category>/YY-MM-DD-name.md`
2. Design approved → Update status to `approved`, add to backlog "Up Next"
3. Start work → Update status to `in-progress`, move to backlog "In Progress"
4. Complete → Create/update system doc, move proposal to `archive/`

**Backlog** (`docs/ai/backlog.md`)
- Single source for "what's next"
- Sections: In Progress, Up Next, Approved (not scheduled), Draft
- Link to proposals, track owner and progress

### When Implementing Features

1. Check `backlog.md` for current priorities
2. Read the relevant proposal for design details
3. Update proposal checklist as you work
4. When done:
   - Create or update the system doc
   - Add changelog entry to system doc
   - Move proposal to `archive/`
   - Remove from backlog

### Granularity Rules
- One behavior/mechanic per system doc
- If a doc exceeds ~200 lines, split it
