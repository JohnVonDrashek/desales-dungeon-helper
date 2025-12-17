---
description: Commit and push all changes immediately
allowed-tools: ["Bash"]
argument-hint: "<commit-message>"
---

Commit all changes and push to remote immediately.

## Steps

1. **Stage everything**
   ```bash
   git add -A
   ```

2. **Show what's being committed**
   ```bash
   git status
   ```

3. **Commit with provided message**
   ```bash
   git commit -m "$ARGUMENTS"
   ```

4. **Push to current branch**
   ```bash
   git push
   ```

No questions. No ceremony. Just commit and push.
