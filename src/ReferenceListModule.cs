using Spectre.Console;
using static MiaLearningSystem.UiHelpers;

namespace MiaLearningSystem;

public static class ReferenceListModule
{
    public static void ManageReferenceLists(DataManager dm)
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[bold blue]MANAGE REFERENCE LISTS[/]").RuleStyle("blue"));

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[bold]Select an option:[/]")
                    .AddChoices("Create Reference List", "View Reference Lists",
                                "Edit Reference List", "Delete Reference List",
                                "Edit Reference List Entries", "← Back")
            );

            switch (choice)
            {
                case "Create Reference List":       CreateReferenceList(dm); break;
                case "View Reference Lists":        ViewReferenceLists(dm); break;
                case "Edit Reference List":         EditReferenceList(dm); break;
                case "Delete Reference List":       DeleteReferenceList(dm); break;
                case "Edit Reference List Entries": SelectAndEditEntries(dm); break;
                case "← Back": return;
            }
        }
    }

    public static void CreateReferenceList(DataManager dm)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]CREATE REFERENCE LIST[/]").RuleStyle("blue"));

        var course = PromptCourse(dm, "\n[bold]Select a course:[/]");
        if (course is null) return;

        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]CREATE REFERENCE LIST[/]").RuleStyle("blue"));
        AnsiConsole.MarkupLine($"\nCourse: [bold]{Markup.Escape(CourseDisplay(course))}[/]");

        var name = AnsiConsole.Prompt(
            new TextPrompt<string>("List name [grey](blank to cancel)[/]:")
                .AllowEmpty()
        );
        if (string.IsNullOrWhiteSpace(name)) return;

        var newList = new ReferenceList(Guid.NewGuid(), course.CourseId, name);
        dm.AddReferenceList(newList);

        AnsiConsole.MarkupLine("\n[green]✓ Reference list created. Opening entry editor...[/]");
        Pause();

        // Automatically enter entry edit mode for the new list.
        ManageReferenceListEntries(dm, newList);
    }

    public static void ViewReferenceLists(DataManager dm)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]VIEW REFERENCE LISTS[/]").RuleStyle("blue"));

        var course = PromptCourse(dm, "\n[bold]Select a course:[/]");
        if (course is null) return;

        var lists = dm.ReferenceLists.Where(r => r.CourseId == course.CourseId).ToList();

        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]VIEW REFERENCE LISTS[/]").RuleStyle("blue"));
        AnsiConsole.MarkupLine($"\nCourse: [bold]{Markup.Escape(CourseDisplay(course))}[/]");

        if (lists.Count == 0)
        {
            AnsiConsole.MarkupLine("\n[yellow]No reference lists found for this course.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"\n[bold]{lists.Count} reference list(s):[/]\n");
            var table = new Table().AddColumn("Name").AddColumn("Entries");
            foreach (var rl in lists)
            {
                int count = dm.ReferenceEntries.Count(e => e.ReferenceListId == rl.ReferenceListId);
                table.AddRow(Markup.Escape(rl.Name), count.ToString());
            }
            AnsiConsole.Write(table);
        }

        Pause();
    }

    public static void EditReferenceList(DataManager dm)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]EDIT REFERENCE LIST[/]").RuleStyle("blue"));

        var course = PromptCourse(dm, "\n[bold]Step 1: Select a course:[/]");
        if (course is null) return;

        var rl = PromptReferenceList(dm, course, "\n[bold]Step 2: Select a reference list to edit:[/]");
        if (rl is null) return;

        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]EDIT REFERENCE LIST[/]").RuleStyle("blue"));
        AnsiConsole.MarkupLine($"\nCourse: [bold]{Markup.Escape(CourseDisplay(course))}[/]");
        AnsiConsole.MarkupLine($"List:   [bold]{Markup.Escape(rl.Name)}[/]");

        var newName = AnsiConsole.Prompt(
            new TextPrompt<string>("\nNew list name [grey](blank to cancel)[/]:")
                .AllowEmpty()
        );
        if (string.IsNullOrWhiteSpace(newName)) return;

        dm.UpdateReferenceList(rl.ReferenceListId, newName);

        AnsiConsole.MarkupLine("\n[green]✓ Reference list updated successfully![/]");
        AnsiConsole.MarkupLine($"  New Name: {Markup.Escape(newName)}");
        Pause();
    }

    public static void DeleteReferenceList(DataManager dm)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]DELETE REFERENCE LIST[/]").RuleStyle("blue"));

        var course = PromptCourse(dm, "\n[bold]Step 1: Select a course:[/]");
        if (course is null) return;

        var rl = PromptReferenceList(dm, course, "\n[bold]Step 2: Select a reference list to delete:[/]");
        if (rl is null) return;

        int entryCount = dm.ReferenceEntries.Count(e => e.ReferenceListId == rl.ReferenceListId);

        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]DELETE REFERENCE LIST[/]").RuleStyle("blue"));
        AnsiConsole.MarkupLine($"\n[yellow]Delete:[/] {Markup.Escape(rl.Name)}");
        AnsiConsole.MarkupLine($"Course: {Markup.Escape(CourseDisplay(course))}");
        if (entryCount > 0)
            AnsiConsole.MarkupLine($"[red]Warning: This will also delete {entryCount} entry/entries in this list.[/]");
        AnsiConsole.MarkupLine("[red]This cannot be undone.[/]");

        var confirm = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("\nConfirm deletion?")
                .AddChoices("Yes, delete", "← Cancel")
        );
        if (confirm != "Yes, delete") return;

        dm.RemoveReferenceList(rl);

        AnsiConsole.MarkupLine("\n[green]✓ Reference list deleted successfully![/]");
        AnsiConsole.MarkupLine($"  {Markup.Escape(rl.Name)} (and {entryCount} entry/entries) removed.");
        Pause();
    }

    // Prompts user to select a list, then enters entry edit mode for it.
    public static void SelectAndEditEntries(DataManager dm)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]EDIT REFERENCE LIST ENTRIES[/]").RuleStyle("blue"));

        var course = PromptCourse(dm, "\n[bold]Step 1: Select a course:[/]");
        if (course is null) return;

        var rl = PromptReferenceList(dm, course, "\n[bold]Step 2: Select a reference list:[/]");
        if (rl is null) return;

        ManageReferenceListEntries(dm, rl);
    }

    // Tiered entry editor: loops on Add/View/Edit/Delete for one specific list.
    public static void ManageReferenceListEntries(DataManager dm, ReferenceList rl)
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold blue]ENTRIES: {Markup.Escape(rl.Name)}[/]").RuleStyle("blue"));

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[bold]Select an option:[/]")
                    .AddChoices("Add Entry", "View Entries", "Edit Entry", "Delete Entry", "← Back")
            );

            switch (choice)
            {
                case "Add Entry":    AddEntry(dm, rl); break;
                case "View Entries": ViewEntries(dm, rl); break;
                case "Edit Entry":   EditEntry(dm, rl); break;
                case "Delete Entry": DeleteEntry(dm, rl); break;
                case "← Back": return;
            }
        }
    }

    public static void AddEntry(DataManager dm, ReferenceList rl)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule($"[bold blue]ENTRIES: {Markup.Escape(rl.Name)}[/]").RuleStyle("blue"));
        AnsiConsole.MarkupLine("\n[bold]Add Entry[/]");

        var term = AnsiConsole.Prompt(
            new TextPrompt<string>("\nTerm [grey](blank to cancel)[/]:")
                .AllowEmpty()
        );
        if (string.IsNullOrWhiteSpace(term)) return;

        var definition = AnsiConsole.Prompt(
            new TextPrompt<string>("Definition [grey](blank to cancel)[/]:")
                .AllowEmpty()
        );
        if (string.IsNullOrWhiteSpace(definition)) return;

        dm.AddReferenceEntry(new ReferenceEntry(Guid.NewGuid(), rl.ReferenceListId, term, definition));

        AnsiConsole.MarkupLine("\n[green]✓ Entry added successfully![/]");
        AnsiConsole.MarkupLine($"  Term:       {Markup.Escape(term)}");
        AnsiConsole.MarkupLine($"  Definition: {Markup.Escape(definition)}");
        Pause();
    }

    public static void ViewEntries(DataManager dm, ReferenceList rl)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule($"[bold blue]ENTRIES: {Markup.Escape(rl.Name)}[/]").RuleStyle("blue"));

        var entries = dm.ReferenceEntries.Where(e => e.ReferenceListId == rl.ReferenceListId).ToList();

        if (entries.Count == 0)
        {
            AnsiConsole.MarkupLine("\n[yellow]No entries in this list.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"\n[bold]{entries.Count} entry/entries:[/]\n");
            var table = new Table().AddColumn("Term").AddColumn("Definition");
            foreach (var e in entries)
                table.AddRow(Markup.Escape(e.Term), Markup.Escape(e.Definition));
            AnsiConsole.Write(table);
        }

        Pause();
    }

    public static void EditEntry(DataManager dm, ReferenceList rl)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule($"[bold blue]ENTRIES: {Markup.Escape(rl.Name)}[/]").RuleStyle("blue"));

        var entry = PromptReferenceEntry(dm, rl, "\n[bold]Select an entry to edit:[/]");
        if (entry is null) return;

        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule($"[bold blue]ENTRIES: {Markup.Escape(rl.Name)}[/]").RuleStyle("blue"));
        AnsiConsole.MarkupLine($"\nTerm:       [bold]{Markup.Escape(entry.Term)}[/]");
        AnsiConsole.MarkupLine($"Definition: [bold]{Markup.Escape(entry.Definition)}[/]");

        var field = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("\n[bold]What would you like to edit?[/]")
                .AddChoices("Term", "Definition", "← Cancel")
        );
        if (field == "← Cancel") return;

        string newTerm = entry.Term;
        string newDefinition = entry.Definition;

        if (field == "Term")
        {
            var input = AnsiConsole.Prompt(
                new TextPrompt<string>("New term [grey](blank to cancel)[/]:")
                    .AllowEmpty()
            );
            if (string.IsNullOrWhiteSpace(input)) return;
            newTerm = input;
        }
        else
        {
            var input = AnsiConsole.Prompt(
                new TextPrompt<string>("New definition [grey](blank to cancel)[/]:")
                    .AllowEmpty()
            );
            if (string.IsNullOrWhiteSpace(input)) return;
            newDefinition = input;
        }

        dm.UpdateReferenceEntry(entry.EntryId, newTerm, newDefinition);

        AnsiConsole.MarkupLine("\n[green]✓ Entry updated successfully![/]");
        AnsiConsole.MarkupLine($"  Term:       {Markup.Escape(newTerm)}");
        AnsiConsole.MarkupLine($"  Definition: {Markup.Escape(newDefinition)}");
        Pause();
    }

    public static void DeleteEntry(DataManager dm, ReferenceList rl)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule($"[bold blue]ENTRIES: {Markup.Escape(rl.Name)}[/]").RuleStyle("blue"));

        var entry = PromptReferenceEntry(dm, rl, "\n[bold]Select an entry to delete:[/]");
        if (entry is null) return;

        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule($"[bold blue]ENTRIES: {Markup.Escape(rl.Name)}[/]").RuleStyle("blue"));
        AnsiConsole.MarkupLine($"\n[yellow]Delete entry:[/]");
        AnsiConsole.MarkupLine($"  Term:       {Markup.Escape(entry.Term)}");
        AnsiConsole.MarkupLine($"  Definition: {Markup.Escape(entry.Definition)}");
        AnsiConsole.MarkupLine("[red]This cannot be undone.[/]");

        var confirm = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("\nConfirm deletion?")
                .AddChoices("Yes, delete", "← Cancel")
        );
        if (confirm != "Yes, delete") return;

        dm.RemoveReferenceEntry(entry);

        AnsiConsole.MarkupLine("\n[green]✓ Entry deleted successfully![/]");
        AnsiConsole.MarkupLine($"  {Markup.Escape(entry.Term)}");
        Pause();
    }
}
