# Project Custom Instructions

## Coding Review Guidelines

When reviewing code, focus on the following points:

- Use of Modern C# Features
  - Consider using the latest syntax available (C# 13 or later), such as `using` declarations, file-scoped namespaces, and collection literals, etc.
- Coding Style
  - Follow general .NET coding guidelines for base conventions (e.g., naming rules).
    - Use `PascalCase` for constant names.
  - Inherit specific coding styles from existing code:
    - Do not use `_` or `s_` prefixes.
    - Omit the `private` modifier.
    - Prefer the use of `var`.
- Unit Tests
  - Check for the presence of unit tests.

Suggest fixes for any sections that deviate from these points.

## Documentation Review Guidelines

When reviewing changes to documentation, focus on the following points:

- Spelling
- Clarity and Conciseness
  - Ensure the content is written in a clear and concise manner.
- Headings
  - Check that appropriate headings are used.
- Documentation Style
  - The writing style must be consistent with existing documentation.
  - Documentation is not a personal blog post. The subject is the project itself, and the project provides content to the reader.
- Sample Code
  - Verify that the code works correctly and maintains quality.
  - Ensure the code follows general .NET coding guidelines (e.g., naming conventions).

If any part deviates from these points, propose a correction.
