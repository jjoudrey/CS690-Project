using Spectre.Console;
using static MiaLearningSystem.UiHelpers;

namespace MiaLearningSystem;

public static class NoteModule
{
    public static void ManageStudyNotes(DataManager dm)
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[bold blue]MANAGE STUDY NOTES[/]").RuleStyle("blue"));

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[bold]Select an option:[/]")
                    .AddChoices("Add Note", "View Notes", "Edit Note", "Delete Note", "← Back")
            );

            switch (choice)
            {
                case "Add Note":    AddNote(dm); break;
                case "View Notes":  ViewNotes(dm); break;
                case "Edit Note":   EditNote(dm); break;
                case "Delete Note": DeleteNote(dm); break;
                case "← Back": return;
            }
        }
    }

    public static void AddNote(DataManager dm)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]ADD NOTE[/]").RuleStyle("blue"));

        var course = PromptCourse(dm, "\n[bold]Step 1: Select a course:[/]");
        if (course is null) return;

        var topic = PromptTopic(dm, course, "\n[bold]Step 2: Select a topic:[/]");
        if (topic is null) return;

        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]ADD NOTE[/]").RuleStyle("blue"));
        AnsiConsole.MarkupLine($"\nCourse: [bold]{Markup.Escape(CourseDisplay(course))}[/]");
        AnsiConsole.MarkupLine($"Topic:  [bold]{Markup.Escape(topic.Name)}[/]");

        var noteName = AnsiConsole.Prompt(
            new TextPrompt<string>("Note name [grey](blank to cancel)[/]:")
                .AllowEmpty()
        );
        if (string.IsNullOrWhiteSpace(noteName)) return;

        string fileName = $"{Guid.NewGuid()}.txt";
        Note note = new Note(Guid.NewGuid(), topic.TopicId, fileName, noteName);
        dm.AddNote(note);

        EditNoteContent(dm, note, course, topic);
    }

    // Live editor: shows last 5 lines. Enter adds a line. Two consecutive blanks finish.
    public static void EditNoteContent(DataManager dm, Note note, Course course, Topic topic)
    {
        const int displayLines = 5;
        bool lastWasBlank = false;

        while (true)
        {
            string[] allLines = DataManager.ReadNoteLines(note, out string _) ?? Array.Empty<string>();
            string[] tail = allLines.Length <= displayLines
                ? allLines
                : allLines[^displayLines..];

            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[bold blue]EDIT NOTE[/]").RuleStyle("blue"));
            AnsiConsole.MarkupLine($"Course: [bold]{Markup.Escape(CourseDisplay(course))}[/]");
            AnsiConsole.MarkupLine($"Topic:  [bold]{Markup.Escape(topic.Name)}[/]");
            AnsiConsole.MarkupLine($"Note:   [bold]{Markup.Escape(note.Name)}[/]");
            AnsiConsole.Write(new Rule().RuleStyle("grey"));

            for (int i = 0; i < displayLines; i++)
            {
                if (i < tail.Length)
                    AnsiConsole.MarkupLine($"  {Markup.Escape(tail[i])}");
                else
                    Console.WriteLine();
            }

            AnsiConsole.Write(new Rule().RuleStyle("grey"));
            AnsiConsole.MarkupLine("[grey]Type a line and press Enter to add it. Press Enter twice on a blank line to finish.[/]");
            Console.Write("> ");

            string? input = Console.ReadLine();
            bool isBlank = string.IsNullOrEmpty(input);

            if (isBlank && lastWasBlank)
                break;

            if (!isBlank)
            {
                DataManager.AppendNoteLine(note, input!);
                lastWasBlank = false;
            }
            else
            {
                lastWasBlank = true;
            }
        }

        AnsiConsole.MarkupLine("\n[green]✓ Note saved successfully![/]");
        AnsiConsole.MarkupLine($"  Course: {Markup.Escape(CourseDisplay(course))}");
        AnsiConsole.MarkupLine($"  Topic:  {Markup.Escape(topic.Name)}");
        AnsiConsole.MarkupLine($"  Note:   {Markup.Escape(note.Name)}");
        Pause();
    }

    public static void ViewNotes(DataManager dm)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]VIEW NOTES[/]").RuleStyle("blue"));

        var course = PromptCourse(dm, "\n[bold]Step 1: Select a course:[/]");
        if (course is null) return;

        var topic = PromptTopic(dm, course, "\n[bold]Step 2: Select a topic:[/]");
        if (topic is null) return;

        var topicNotes = dm.Notes.Where(n => n.TopicId == topic.TopicId).ToList();

        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]VIEW NOTES[/]").RuleStyle("blue"));
        AnsiConsole.MarkupLine($"\nCourse: [bold]{Markup.Escape(CourseDisplay(course))}[/]");
        AnsiConsole.MarkupLine($"Topic:  [bold]{Markup.Escape(topic.Name)}[/]");

        if (topicNotes.Count == 0)
        {
            AnsiConsole.MarkupLine("\n[yellow]No notes found for this topic.[/]");
            Pause();
            return;
        }

        const string BACK = "← Back";
        var labels = topicNotes.Select(n => n.Name).Append(BACK).ToList();

        var sel = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("\n[bold]Select a note to view:[/]")
                .UseConverter(s => s == BACK ? s : Markup.Escape(s))
                .AddChoices(labels)
        );

        if (sel == BACK) return;

        int idx = labels.IndexOf(sel);
        ViewNoteContent(dm, topicNotes[idx], course, topic);
    }

    public static void ViewNoteContent(DataManager dm, Note note, Course course, Topic topic)
    {
        string[]? lines = DataManager.ReadNoteLines(note, out string err);

        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]VIEW NOTE[/]").RuleStyle("blue"));
        AnsiConsole.MarkupLine($"Course: [bold]{Markup.Escape(CourseDisplay(course))}[/]");
        AnsiConsole.MarkupLine($"Topic:  [bold]{Markup.Escape(topic.Name)}[/]");
        AnsiConsole.MarkupLine($"Note:   [bold]{Markup.Escape(note.Name)}[/]");
        AnsiConsole.Write(new Rule().RuleStyle("grey"));

        if (lines is null)
        {
            AnsiConsole.MarkupLine($"[red]ERROR: {Markup.Escape(err)}[/]");
        }
        else if (lines.Length == 0)
        {
            AnsiConsole.MarkupLine("[grey](Note is empty)[/]");
        }
        else
        {
            const int maxDisplay = 8;
            int start = lines.Length > maxDisplay ? lines.Length - maxDisplay : 0;
            if (start > 0)
                AnsiConsole.MarkupLine($"[grey]  ... ({start} earlier line(s) not shown)[/]");
            for (int i = start; i < lines.Length; i++)
                AnsiConsole.MarkupLine($"  {Markup.Escape(lines[i])}");
        }

        AnsiConsole.Write(new Rule().RuleStyle("grey"));
        Pause();
    }

    public static void EditNote(DataManager dm)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]EDIT NOTE[/]").RuleStyle("blue"));

        var course = PromptCourse(dm, "\n[bold]Step 1: Select a course:[/]");
        if (course is null) return;

        var topic = PromptTopic(dm, course, "\n[bold]Step 2: Select a topic:[/]");
        if (topic is null) return;

        var topicNotes = dm.Notes.Where(n => n.TopicId == topic.TopicId).ToList();
        if (topicNotes.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No notes found for this topic.[/]");
            Pause();
            return;
        }

        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]EDIT NOTE[/]").RuleStyle("blue"));
        AnsiConsole.MarkupLine($"\nCourse: [bold]{Markup.Escape(CourseDisplay(course))}[/]");
        AnsiConsole.MarkupLine($"Topic:  [bold]{Markup.Escape(topic.Name)}[/]");

        const string BACK = "← Back";
        var noteLabels = topicNotes.Select(n => n.Name).Append(BACK).ToList();
        var noteSel = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("\n[bold]Step 3: Select a note to edit:[/]")
                .UseConverter(s => s == BACK ? s : Markup.Escape(s))
                .AddChoices(noteLabels)
        );
        if (noteSel == BACK) return;

        var note = topicNotes[noteLabels.IndexOf(noteSel)];

        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]EDIT NOTE[/]").RuleStyle("blue"));
        AnsiConsole.MarkupLine($"\nCourse: [bold]{Markup.Escape(CourseDisplay(course))}[/]");
        AnsiConsole.MarkupLine($"Topic:  [bold]{Markup.Escape(topic.Name)}[/]");
        AnsiConsole.MarkupLine($"Note:   [bold]{Markup.Escape(note.Name)}[/]");

        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("\n[bold]What would you like to do?[/]")
                .AddChoices("Edit Content", "Rename Note", "← Cancel")
        );

        if (action == "← Cancel") return;

        if (action == "Edit Content")
        {
            EditNoteContent(dm, note, course, topic);
        }
        else
        {
            var newName = AnsiConsole.Prompt(
                new TextPrompt<string>("New note name [grey](blank to cancel)[/]:")
                    .AllowEmpty()
            );
            if (string.IsNullOrWhiteSpace(newName)) return;

            dm.UpdateNote(note.NoteId, newName);

            AnsiConsole.MarkupLine("\n[green]✓ Note renamed successfully![/]");
            AnsiConsole.MarkupLine($"  New Name: {Markup.Escape(newName)}");
            Pause();
        }
    }

    public static void DeleteNote(DataManager dm)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]DELETE NOTE[/]").RuleStyle("blue"));

        var course = PromptCourse(dm, "\n[bold]Step 1: Select a course:[/]");
        if (course is null) return;

        var topic = PromptTopic(dm, course, "\n[bold]Step 2: Select a topic:[/]");
        if (topic is null) return;

        var topicNotes = dm.Notes.Where(n => n.TopicId == topic.TopicId).ToList();
        if (topicNotes.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No notes found for this topic.[/]");
            Pause();
            return;
        }

        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]DELETE NOTE[/]").RuleStyle("blue"));
        AnsiConsole.MarkupLine($"\nCourse: [bold]{Markup.Escape(CourseDisplay(course))}[/]");
        AnsiConsole.MarkupLine($"Topic:  [bold]{Markup.Escape(topic.Name)}[/]");

        const string BACK = "← Back";
        var noteLabels = topicNotes.Select(n => n.Name).Append(BACK).ToList();
        var noteSel = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("\n[bold]Step 3: Select a note to delete:[/]")
                .UseConverter(s => s == BACK ? s : Markup.Escape(s))
                .AddChoices(noteLabels)
        );
        if (noteSel == BACK) return;

        var note = topicNotes[noteLabels.IndexOf(noteSel)];

        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]DELETE NOTE[/]").RuleStyle("blue"));
        AnsiConsole.MarkupLine($"\n[yellow]Delete:[/] {Markup.Escape(note.Name)}");
        AnsiConsole.MarkupLine($"Course: {Markup.Escape(CourseDisplay(course))}");
        AnsiConsole.MarkupLine($"Topic:  {Markup.Escape(topic.Name)}");
        AnsiConsole.MarkupLine("[red]This cannot be undone.[/]");

        var confirm = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("\nConfirm deletion?")
                .AddChoices("Yes, delete", "← Cancel")
        );
        if (confirm != "Yes, delete") return;

        dm.RemoveNote(note);

        AnsiConsole.MarkupLine("\n[green]✓ Note deleted successfully![/]");
        AnsiConsole.MarkupLine($"  {Markup.Escape(note.Name)}");
        Pause();
    }
}
