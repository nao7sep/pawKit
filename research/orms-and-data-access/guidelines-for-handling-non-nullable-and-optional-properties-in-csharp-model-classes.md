<!-- 2025-04-02T08:53:43Z -->

# Guidelines for Handling Non-Nullable and Optional Properties in C# Model Classes

This guideline focuses on **C# model classes**, **nullable reference types (NRT)**, and how to handle **required vs. optional** (non-nullable vs. nullable) properties — especially when matching with a database schema.

## 1. Non-Nullable vs. Nullable

### 1.1 Database Schema Consideration

- If a **database column** is **NOT NULL**, the corresponding C# property should be a **non-nullable** reference type.
- If a database column **can be `NULL`**, the property should be **nullable** (`string?`).

#### Example

```csharp
public class User
{
    // Column: FirstName (NOT NULL in DB)
    // => Non-nullable in C#
    public string FirstName { get; set; } = null!;

    // Column: MiddleName (NULL in DB)
    // => Nullable in C#
    public string? MiddleName { get; set; }
}
```

> **Note**: `= null!;` is called the **null-forgiving operator**. It silences compiler warnings by telling the compiler, “I know this will not actually be null at runtime.”

## 2. Using the `required` Keyword (C# 11+)

### 2.1 What `required` Does
Marking a property with `required` forces it to be set **during object initialization** or in a **constructor**. For example:

```csharp
public class Customer
{
    public required string Name { get; set; }
}
```

When you instantiate this class, you must immediately set `Name`:

```csharp
// ❌ Compile-time error: required property not set
var customer = new Customer();

// ✅ Must set it here
var customer = new Customer
{
    Name = "Alice"
};
```

### 2.2 When `required` Isn’t Suitable
If you **don’t** have the value at the time of creation or you plan to **populate** the property later (e.g., a factory method, EF Core materialization), `required` will be too strict.

In those cases, using a **non-nullable property with `null!`** is more flexible:

```csharp
public string Name { get; set; } = null!;
```

## 3. Strategy for Non-Optional (Non-Nullable) Fields

When you know a property must never be `null` (e.g., a required DB column), you typically have **two** major approaches:

1. **Constructor or `required`**
   - Enforce at compile time with a **constructor parameter** or with `required`.
   - Example:
     ```csharp
     public class User
     {
         public User(string firstName)
         {
             FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
         }

         public string FirstName { get; }
     }
     ```
   - Or (C# 11+):
     ```csharp
     public class User
     {
         public required string FirstName { get; set; }
     }
     ```
   - **Pros**: No possibility of an uninitialized property at runtime.
   - **Cons**: Might not be suitable if initialization happens partially or via a factory.

2. **Use `null!`**
   - **Most consistent approach** if initialization is delayed or external (e.g., via ORM or a factory).
   - Example:
     ```csharp
     public class User
     {
         public string FirstName { get; set; } = null!;
     }
     ```
   - **Pros**: Flexible; no compile-time error for partial builds.
   - **Cons**: You lose compile-time checks that the property was set.

## 4. Strategy for Optional (Nullable) Fields

If a property is optional and can be `NULL` in the database, then mark it **nullable** in C#:

```csharp
public string? MiddleName { get; set; }
```

- **Pro**: Matches the database schema accurately.
- **Con**: You have to handle `null` checks in your code.

## 5. Putting It All Together

Here’s a sample **model class** that lines up with a typical database schema:

```csharp
public class User
{
    // Id is non-nullable integer in DB
    public int Id { get; set; }

    // FirstName is NOT NULL in DB
    // We either require it at creation or use null-forgiving
    public string FirstName { get; set; } = null!;

    // MiddleName is NULL in DB
    public string? MiddleName { get; set; }

    // LastName is required at creation using the 'required' keyword (C# 11+)
    public required string LastName { get; set; }
}
```

Depending on your project constraints and how you create/populate your objects, you can mix or match these approaches. For example, if `LastName` is also set by a factory or EF, you may need to remove `required` and use `null!` instead.

## 6. Summary

1. **Non-Nullable (DB NOT NULL)**
   - Use **non-nullable reference type** + `null!` if initialization can happen later.
   - Or **constructor injection** / `required` if you want strict compile-time checks.

2. **Nullable (DB NULL)**
   - Use **nullable reference type** (`string?`) and handle `null` carefully in code.

3. **Use `required`** only if you can (and want to) set properties **immediately** during object creation. Otherwise, `null!` is the practical choice in many real-world scenarios (e.g., EF Core, factories).
