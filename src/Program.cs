using System.Globalization;
using MiaLearningSystem;
using Spectre.Console;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding  = System.Text.Encoding.UTF8;

var dm = new DataManager();

while (true)
{
    AnsiConsole.Clear();
    AnsiConsole.Write(new Rule("[bold blue]MIA LEARNING SYSTEM[/]").RuleStyle("blue"));

    var choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("\n[bold]What would you like to do?[/]")
            .AddChoices("Manage Courses", "Organize Topics", "Manage Study Notes",
                        "Manage Reference Lists", "Track Assessments", "Exit")
    );

    switch (choice)
    {
        case "Manage Courses":        ManageCourses(dm); break;
        case "Organize Topics":       OrganizeTopics(dm); break;
        case "Manage Study Notes":    ManageStudyNotes(dm); break;
        case "Manage Reference Lists": ManageReferenceLists(dm); break;
        case "Track Assessments":     TrackAssessments(dm); break;
        case "Exit":
            AnsiConsole.MarkupLine("[grey]Goodbye![/]");
            return;
    }
}

// ── Courses ───────────────────────────────────────────────────────────────────

static void ManageCourses(DataManager dm)
{
    while (true)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]MANAGE COURSES[/]").RuleStyle("blue"));

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("\n[bold]Select an option:[/]")
                .AddChoices("Create Course", "View Courses", "Edit Course", "Delete Course", "← Back")
        );

        switch (choice)
        {
            case "Create Course": CreateCourse(dm); break;
            case "View Courses":  ViewCourses(dm); break;
            case "Edit Course":   EditCourse(dm); break;
            case "Delete Course": DeleteCourse(dm); break;
            case "← Back": return;
        }
    }
}

static void CreateCourse(DataManager dm)
{
    AnsiConsole.Clear();
    AnsiConsole.Write(new Rule("[bold blue]CREATE NEW COURSE[/]").RuleStyle("blue"));

    var name = AnsiConsole.Prompt(
        new TextPrompt<string>("\nCourse name [grey](blank to cancel)[/]:")
            .AllowEmpty()
    );
    if (string.IsNullOrWhiteSpace(name)) return;

    var subjectArea = AnsiConsole.Prompt(
        new TextPrompt<string>("Subject area [grey](blank to cancel)[/]:")
            .AllowEmpty()
    );
    if (string.IsNullOrWhiteSpace(subjectArea)) return;

    dm.AddCourse(new Course(Guid.NewGuid(), name, subjectArea));

    AnsiConsole.MarkupLine("\n[green]✓ Course created successfully![/]");
    AnsiConsole.MarkupLine($"  Name:         {Markup.Escape(name)}");
    AnsiConsole.MarkupLine($"  Subject Area: {Markup.Escape(subjectArea)}");
    AnsiConsole.MarkupLine($"  Created:      {DateTime.Today:yyyy-MM-dd}");
    Pause();
}

static void ViewCourses(DataManager dm)
{
    AnsiConsole.Clear();
    AnsiConsole.Write(new Rule("[bold blue]VIEW COURSES[/]").RuleStyle("blue"));

    if (dm.Courses.Count == 0)
    {
        AnsiConsole.MarkupLine("\n[yellow]No courses found.[/]");
    }
    else
    {
        AnsiConsole.MarkupLine($"\n[bold]{dm.Courses.Count} course(s):[/]");
        for (int i = 0; i < dm.Courses.Count; i++)
            AnsiConsole.MarkupLine($"  {i + 1}. {Markup.Escape(CourseDisplay(dm.Courses[i]))}");
    }

    Pause();
}

static void EditCourse(DataManager dm)
{
    AnsiConsole.Clear();
    AnsiConsole.Write(new Rule("[bold blue]EDIT COURSE[/]").RuleStyle("blue"));

    var course = PromptCourse(dm, "\n[bold]Select a course to edit:[/]");
    if (course is null) return;

    AnsiConsole.Clear();
    AnsiConsole.Write(new Rule("[bold blue]EDIT COURSE[/]").RuleStyle("blue"));
    AnsiConsole.MarkupLine($"\nEditing: [bold]{Markup.Escape(CourseDisplay(course))}[/]");
    AnsiConsole.MarkupLine($"  Name:         {Markup.Escape(course.Name)}");
    AnsiConsole.MarkupLine($"  Subject Area: {Markup.Escape(course.SubjectArea)}");

    var field = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("\n[bold]What would you like to change?[/]")
            .AddChoices("Course Name", "Subject Area", "← Cancel")
    );

    if (field == "← Cancel") return;

    string newName = course.Name;
    string newSubjectArea = course.SubjectArea;

    if (field == "Course Name")
    {
        var input = AnsiConsole.Prompt(
            new TextPrompt<string>("New course name [grey](blank to cancel)[/]:")
                .AllowEmpty()
        );
        if (string.IsNullOrWhiteSpace(input)) return;
        newName = input;
    }
    else
    {
        var input = AnsiConsole.Prompt(
            new TextPrompt<string>("New subject area [grey](blank to cancel)[/]:")
                .AllowEmpty()
        );
        if (string.IsNullOrWhiteSpace(input)) return;
        newSubjectArea = input;
    }

    dm.UpdateCourse(course.CourseId, newName, newSubjectArea);

    AnsiConsole.MarkupLine("\n[green]✓ Course updated successfully![/]");
    AnsiConsole.MarkupLine($"  Name:         {Markup.Escape(newName)}");
    AnsiConsole.MarkupLine($"  Subject Area: {Markup.Escape(newSubjectArea)}");
    Pause();
}

static void DeleteCourse(DataManager dm)
{
    AnsiConsole.Clear();
    AnsiConsole.Write(new Rule("[bold blue]DELETE COURSE[/]").RuleStyle("blue"));

    var course = PromptCourse(dm, "\n[bold]Select a course to delete:[/]");
    if (course is null) return;

    AnsiConsole.Clear();
    AnsiConsole.Write(new Rule("[bold blue]DELETE COURSE[/]").RuleStyle("blue"));
    AnsiConsole.MarkupLine($"\n[yellow]Delete:[/] {Markup.Escape(CourseDisplay(course))}");
    AnsiConsole.MarkupLine("[red]This cannot be undone.[/]");

    var confirm = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("\nConfirm deletion?")
            .AddChoices("Yes, delete", "← Cancel")
    );

    if (confirm != "Yes, delete") return;

    dm.RemoveCourse(course);

    AnsiConsole.MarkupLine("\n[green]✓ Course deleted successfully![/]");
    AnsiConsole.MarkupLine($"  {Markup.Escape(CourseDisplay(course))}");
    Pause();
}

// ── Topics ────────────────────────────────────────────────────────────────────

static void OrganizeTopics(DataManager dm)
{
    while (true)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]ORGANIZE TOPICS[/]").RuleStyle("blue"));

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("\n[bold]Select an option:[/]")
                .AddChoices("Add Topic", "View Topics", "Edit Topic", "Delete Topic", "← Back")
        );

        switch (choice)
        {
            case "Add Topic":    AddTopic(dm); break;
            case "View Topics":  ViewTopics(dm); break;
            case "Edit Topic":   EditTopic(dm); break;
            case "Delete Topic": DeleteTopic(dm); break;
            case "← Back": return;
        }
    }
}

static void AddTopic(DataManager dm)
{
    AnsiConsole.Clear();
    AnsiConsole.Write(new Rule("[bold blue]ADD TOPIC[/]").RuleStyle("blue"));

    var course = PromptCourse(dm, "\n[bold]Select a course:[/]");
    if (course is null) return;

    AnsiConsole.Clear();
    AnsiConsole.Write(new Rule("[bold blue]ADD TOPIC[/]").RuleStyle("blue"));
    AnsiConsole.MarkupLine($"\nCourse: [bold]{Markup.Escape(CourseDisplay(course))}[/]");

    var topicName = AnsiConsole.Prompt(
        new TextPrompt<string>("Topic name [grey](blank to cancel)[/]:")
            .AllowEmpty()
    );
    if (string.IsNullOrWhiteSpace(topicName)) return;

    dm.AddTopic(new Topic(Guid.NewGuid(), course.CourseId, topicName));

    AnsiConsole.MarkupLine("\n[green]✓ Topic added successfully![/]");
    AnsiConsole.MarkupLine($"  Course:     {Markup.Escape(CourseDisplay(course))}");
    AnsiConsole.MarkupLine($"  Topic Name: {Markup.Escape(topicName)}");
    Pause();
}

static void ViewTopics(DataManager dm)
{
    AnsiConsole.Clear();
    AnsiConsole.Write(new Rule("[bold blue]VIEW TOPICS[/]").RuleStyle("blue"));

    var course = PromptCourse(dm, "\n[bold]Select a course to view topics:[/]");
    if (course is null) return;

    var courseTopics = dm.Topics.Where(t => t.CourseId == course.CourseId).ToList();

    AnsiConsole.Clear();
    AnsiConsole.Write(new Rule("[bold blue]VIEW TOPICS[/]").RuleStyle("blue"));
    AnsiConsole.MarkupLine($"\nCourse: [bold]{Markup.Escape(CourseDisplay(course))}[/]");

    if (courseTopics.Count == 0)
    {
        AnsiConsole.MarkupLine("[yellow]No topics found for this course.[/]");
    }
    else
    {
        AnsiConsole.MarkupLine($"\n[bold]{courseTopics.Count} topic(s):[/]");
        for (int i = 0; i < courseTopics.Count; i++)
            AnsiConsole.MarkupLine($"  {i + 1}. {Markup.Escape(courseTopics[i].Name)}");
    }

    Pause();
}

static void EditTopic(DataManager dm)
{
    AnsiConsole.Clear();
    AnsiConsole.Write(new Rule("[bold blue]EDIT TOPIC[/]").RuleStyle("blue"));

    var course = PromptCourse(dm, "\n[bold]Step 1: Select a course:[/]");
    if (course is null) return;

    var topic = PromptTopic(dm, course, "\n[bold]Step 2: Select a topic to edit:[/]");
    if (topic is null) return;

    AnsiConsole.Clear();
    AnsiConsole.Write(new Rule("[bold blue]EDIT TOPIC[/]").RuleStyle("blue"));
    AnsiConsole.MarkupLine($"\nCourse: [bold]{Markup.Escape(CourseDisplay(course))}[/]");
    AnsiConsole.MarkupLine($"Topic:  [bold]{Markup.Escape(topic.Name)}[/]");

    var newName = AnsiConsole.Prompt(
        new TextPrompt<string>("\nNew topic name [grey](blank to cancel)[/]:")
            .AllowEmpty()
    );
    if (string.IsNullOrWhiteSpace(newName)) return;

    dm.UpdateTopic(topic.TopicId, newName);

    AnsiConsole.MarkupLine("\n[green]✓ Topic updated successfully![/]");
    AnsiConsole.MarkupLine($"  Course:    {Markup.Escape(CourseDisplay(course))}");
    AnsiConsole.MarkupLine($"  New Name:  {Markup.Escape(newName)}");
    Pause();
}

static void DeleteTopic(DataManager dm)
{
    AnsiConsole.Clear();
    AnsiConsole.Write(new Rule("[bold blue]DELETE TOPIC[/]").RuleStyle("blue"));

    var course = PromptCourse(dm, "\n[bold]Step 1: Select a course:[/]");
    if (course is null) return;

    var topic = PromptTopic(dm, course, "\n[bold]Step 2: Select a topic to delete:[/]");
    if (topic is null) return;

    var childNotes = dm.Notes.Where(n => n.TopicId == topic.TopicId).ToList();

    AnsiConsole.Clear();
    AnsiConsole.Write(new Rule("[bold blue]DELETE TOPIC[/]").RuleStyle("blue"));
    AnsiConsole.MarkupLine($"\n[yellow]Delete:[/] {Markup.Escape(topic.Name)}");
    AnsiConsole.MarkupLine($"Course: {Markup.Escape(CourseDisplay(course))}");
    if (childNotes.Count > 0)
        AnsiConsole.MarkupLine($"[red]Warning: This will also delete {childNotes.Count} note(s) under this topic.[/]");
    AnsiConsole.MarkupLine("[red]This cannot be undone.[/]");

    var confirm = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("\nConfirm deletion?")
            .AddChoices("Yes, delete", "← Cancel")
    );
    if (confirm != "Yes, delete") return;

    foreach (var note in childNotes)
        dm.RemoveNote(note);
    dm.RemoveTopic(topic);

    AnsiConsole.MarkupLine("\n[green]✓ Topic deleted successfully![/]");
    AnsiConsole.MarkupLine($"  {Markup.Escape(topic.Name)} (and {childNotes.Count} note(s)) removed.");
    Pause();
}

// ── Study Notes ───────────────────────────────────────────────────────────────

static void ManageStudyNotes(DataManager dm)
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

static void AddNote(DataManager dm)
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
static void EditNoteContent(DataManager dm, Note note, Course course, Topic topic)
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

static void ViewNotes(DataManager dm)
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

static void ViewNoteContent(DataManager dm, Note note, Course course, Topic topic)
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

static void EditNote(DataManager dm)
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

static void DeleteNote(DataManager dm)
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

// ── Reference Lists ───────────────────────────────────────────────────────────

static void ManageReferenceLists(DataManager dm)
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

static void CreateReferenceList(DataManager dm)
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

static void ViewReferenceLists(DataManager dm)
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

static void EditReferenceList(DataManager dm)
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

static void DeleteReferenceList(DataManager dm)
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
static void SelectAndEditEntries(DataManager dm)
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
static void ManageReferenceListEntries(DataManager dm, ReferenceList rl)
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

static void AddEntry(DataManager dm, ReferenceList rl)
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

static void ViewEntries(DataManager dm, ReferenceList rl)
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

static void EditEntry(DataManager dm, ReferenceList rl)
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

static void DeleteEntry(DataManager dm, ReferenceList rl)
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

// ── Assessments ───────────────────────────────────────────────────────────────

static void TrackAssessments(DataManager dm)
{
    while (true)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]TRACK UPCOMING ASSESSMENTS[/]").RuleStyle("blue"));

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("\n[bold]Select an option:[/]")
                .AddChoices("Create Quiz", "View Quizzes", "Edit Quiz", "Delete Quiz", "← Back")
        );

        switch (choice)
        {
            case "Create Quiz":  CreateQuiz(dm); break;
            case "View Quizzes": ViewQuizzes(dm); break;
            case "Edit Quiz":    EditQuiz(dm); break;
            case "Delete Quiz":  DeleteQuiz(dm); break;
            case "← Back": return;
        }
    }
}

static void CreateQuiz(DataManager dm)
{
    AnsiConsole.Clear();
    AnsiConsole.Write(new Rule("[bold blue]CREATE QUIZ[/]").RuleStyle("blue"));

    var quizName = AnsiConsole.Prompt(
        new TextPrompt<string>("\n[bold]Step 1:[/] Quiz name [grey](blank to cancel)[/]:")
            .AllowEmpty()
    );
    if (string.IsNullOrWhiteSpace(quizName)) return;

    var dateStr = AnsiConsole.Prompt(
        new TextPrompt<string>("[bold]Step 2:[/] Due date [grey](yyyy-MM-dd, blank to cancel)[/]:")
            .AllowEmpty()
            .Validate(s => string.IsNullOrWhiteSpace(s) ||
                           DateOnly.TryParse(s, CultureInfo.InvariantCulture, out _)
                ? ValidationResult.Success()
                : ValidationResult.Error("[red]Use format yyyy-MM-dd[/]"))
    );
    if (string.IsNullOrWhiteSpace(dateStr)) return;
    DateOnly.TryParse(dateStr, CultureInfo.InvariantCulture, out DateOnly dueDate);

    var course = PromptCourse(dm, "[bold]Step 3:[/] Select a course:");
    if (course is null) return;

    var courseTopics = dm.Topics.Where(t => t.CourseId == course.CourseId).ToList();
    List<Topic> selectedTopics;

    if (courseTopics.Count == 0)
    {
        AnsiConsole.MarkupLine("\n[yellow]No topics for this course. Quiz will have no topic scope.[/]");
        selectedTopics = [];
    }
    else
    {
        selectedTopics = AnsiConsole.Prompt(
            new MultiSelectionPrompt<Topic>()
                .Title("[bold]Step 4:[/] Select topics for scope [grey](Space to toggle, Enter to confirm)[/]:")
                .NotRequired()
                .UseConverter(t => Markup.Escape(t.Name))
                .AddChoices(courseTopics)
        );
    }

    Quiz quiz = new Quiz(Guid.NewGuid(), course.CourseId, quizName, dueDate, false);
    dm.AddQuiz(quiz);
    foreach (var t in selectedTopics)
        dm.AddQuizTopic(new QuizTopic(Guid.NewGuid(), quiz.QuizId, t.TopicId));

    AnsiConsole.MarkupLine("\n[green]✓ Quiz created successfully![/]");
    AnsiConsole.MarkupLine($"  Quiz Name: {Markup.Escape(quizName)}");
    AnsiConsole.MarkupLine($"  Course:    {Markup.Escape(CourseDisplay(course))}");
    AnsiConsole.MarkupLine($"  Due Date:  {dueDate:yyyy-MM-dd}");
    AnsiConsole.MarkupLine($"  Topics:    {selectedTopics.Count} selected");
    Pause();
}

static void ViewQuizzes(DataManager dm)
{
    AnsiConsole.Clear();
    AnsiConsole.Write(new Rule("[bold blue]VIEW QUIZZES[/]").RuleStyle("blue"));

    if (dm.Quizzes.Count == 0)
    {
        AnsiConsole.MarkupLine("\n[yellow]No quizzes found.[/]");
        Pause();
        return;
    }

    var sorted = dm.Quizzes.OrderBy(q => q.DueDate).ToList();
    const string BACK = "← Back";
    var labels = sorted.Select(q =>
    {
        string status = q.IsCompleted ? "✓" : " ";
        return $"[{status}] {q.Name}  (due {q.DueDate:yyyy-MM-dd})";
    }).Append(BACK).ToList();

    var sel = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("\n[bold]View Quizzes[/] — select one to open:")
            .UseConverter(s => s == BACK ? s : Markup.Escape(s))
            .AddChoices(labels)
    );

    if (sel == BACK) return;

    int idx = labels.IndexOf(sel);
    ViewQuizDetail(dm, sorted[idx]);
}

static void ViewQuizDetail(DataManager dm, Quiz quiz)
{
    Course? course = dm.Courses.FirstOrDefault(c => c.CourseId == quiz.CourseId);
    var quizTopicIds = dm.QuizTopics
        .Where(qt => qt.QuizId == quiz.QuizId)
        .Select(qt => qt.TopicId)
        .ToHashSet();
    var scopeTopics = dm.Topics.Where(t => quizTopicIds.Contains(t.TopicId)).ToList();

    AnsiConsole.Clear();
    AnsiConsole.Write(new Rule("[bold blue]QUIZ DETAIL[/]").RuleStyle("blue"));
    AnsiConsole.MarkupLine($"\nName:   [bold]{Markup.Escape(quiz.Name)}[/]");
    AnsiConsole.MarkupLine($"Course: {Markup.Escape(course != null ? CourseDisplay(course) : "Unknown")}");
    AnsiConsole.MarkupLine($"Due:    {quiz.DueDate:yyyy-MM-dd}");
    AnsiConsole.MarkupLine($"Status: {(quiz.IsCompleted ? "[green]✓ Completed[/]" : "[yellow]Pending[/]")}");

    if (scopeTopics.Count == 0)
        AnsiConsole.MarkupLine("Scope:  (none)");
    else
        AnsiConsole.MarkupLine($"Scope:  {Markup.Escape(string.Join(", ", scopeTopics.Select(t => t.Name)))}");

    Pause();
}

static void EditQuiz(DataManager dm)
{
    AnsiConsole.Clear();
    AnsiConsole.Write(new Rule("[bold blue]EDIT QUIZ[/]").RuleStyle("blue"));

    var quiz = PromptQuiz(dm, "\n[bold]Select a quiz to edit:[/]");
    if (quiz is null) return;

    AnsiConsole.Clear();
    AnsiConsole.Write(new Rule("[bold blue]EDIT QUIZ[/]").RuleStyle("blue"));
    AnsiConsole.MarkupLine($"\nEditing: [bold]{Markup.Escape(quiz.Name)}[/]");
    AnsiConsole.MarkupLine($"  Due Date: {quiz.DueDate:yyyy-MM-dd}");

    var choices = new List<string>();
    if (!quiz.IsCompleted) choices.Add("Mark as Completed");
    choices.AddRange(["Quiz Name", "Due Date", "Edit Topic Scope", "← Cancel"]);

    var field = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("\n[bold]What would you like to change?[/]")
            .AddChoices(choices)
    );
    if (field == "← Cancel") return;

    if (field == "Quiz Name")
    {
        var input = AnsiConsole.Prompt(
            new TextPrompt<string>("New quiz name [grey](blank to cancel)[/]:")
                .AllowEmpty()
        );
        if (string.IsNullOrWhiteSpace(input)) return;
        dm.UpdateQuiz(quiz.QuizId, input, quiz.DueDate);
        AnsiConsole.MarkupLine("\n[green]✓ Quiz updated successfully![/]");
        AnsiConsole.MarkupLine($"  Name:     {Markup.Escape(input)}");
        AnsiConsole.MarkupLine($"  Due Date: {quiz.DueDate:yyyy-MM-dd}");
    }
    else if (field == "Due Date")
    {
        var dateStr = AnsiConsole.Prompt(
            new TextPrompt<string>("New due date [grey](yyyy-MM-dd, blank to cancel)[/]:")
                .AllowEmpty()
                .Validate(s => string.IsNullOrWhiteSpace(s) ||
                               DateOnly.TryParse(s, CultureInfo.InvariantCulture, out _)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Use format yyyy-MM-dd[/]"))
        );
        if (string.IsNullOrWhiteSpace(dateStr)) return;
        DateOnly.TryParse(dateStr, CultureInfo.InvariantCulture, out DateOnly newDueDate);
        dm.UpdateQuiz(quiz.QuizId, quiz.Name, newDueDate);
        AnsiConsole.MarkupLine("\n[green]✓ Quiz updated successfully![/]");
        AnsiConsole.MarkupLine($"  Name:     {Markup.Escape(quiz.Name)}");
        AnsiConsole.MarkupLine($"  Due Date: {newDueDate:yyyy-MM-dd}");
    }
    else if (field == "Mark as Completed")
    {
        dm.MarkQuizCompleted(quiz.QuizId);
        AnsiConsole.MarkupLine("\n[green]✓ Quiz marked as completed![/]");
        AnsiConsole.MarkupLine($"  {Markup.Escape(quiz.Name)}");
    }
    else // Edit Topic Scope
    {
        Course? quizCourse = dm.Courses.FirstOrDefault(c => c.CourseId == quiz.CourseId);
        if (quizCourse is null)
        {
            AnsiConsole.MarkupLine("[red]Cannot edit scope: the course for this quiz no longer exists.[/]");
            Pause();
            return;
        }

        var courseTopics = dm.Topics.Where(t => t.CourseId == quiz.CourseId).ToList();
        if (courseTopics.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No topics exist for this quiz's course.[/]");
            Pause();
            return;
        }

        var currentTopicIds = dm.QuizTopics
            .Where(qt => qt.QuizId == quiz.QuizId)
            .Select(qt => qt.TopicId)
            .ToHashSet();

        var scopePrompt = new MultiSelectionPrompt<Topic>()
            .Title("Select topics for scope [grey](Space to toggle, Enter to confirm)[/]:")
            .NotRequired()
            .UseConverter(t => Markup.Escape(t.Name))
            .AddChoices(courseTopics);

        foreach (var t in courseTopics.Where(t => currentTopicIds.Contains(t.TopicId)))
            scopePrompt.Select(t);

        var selectedTopics = AnsiConsole.Prompt(scopePrompt);
        dm.ReplaceQuizTopics(quiz.QuizId, selectedTopics.Select(t => t.TopicId));

        AnsiConsole.MarkupLine("\n[green]✓ Quiz scope updated successfully![/]");
        AnsiConsole.MarkupLine($"  Topics: {selectedTopics.Count} selected");
    }

    Pause();
}

static void DeleteQuiz(DataManager dm)
{
    AnsiConsole.Clear();
    AnsiConsole.Write(new Rule("[bold blue]DELETE QUIZ[/]").RuleStyle("blue"));

    var quiz = PromptQuiz(dm, "\n[bold]Select a quiz to delete:[/]");
    if (quiz is null) return;

    int topicCount = dm.QuizTopics.Count(qt => qt.QuizId == quiz.QuizId);

    AnsiConsole.Clear();
    AnsiConsole.Write(new Rule("[bold blue]DELETE QUIZ[/]").RuleStyle("blue"));
    AnsiConsole.MarkupLine($"\n[yellow]Delete:[/] {Markup.Escape(quiz.Name)}");
    AnsiConsole.MarkupLine($"Due Date: {quiz.DueDate:yyyy-MM-dd}");
    if (topicCount > 0)
        AnsiConsole.MarkupLine($"[red]Warning: This will also remove {topicCount} topic scope assignment(s).[/]");
    AnsiConsole.MarkupLine("[red]This cannot be undone.[/]");

    var confirm = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("\nConfirm deletion?")
            .AddChoices("Yes, delete", "← Cancel")
    );
    if (confirm != "Yes, delete") return;

    dm.RemoveQuiz(quiz);

    AnsiConsole.MarkupLine("\n[green]✓ Quiz deleted successfully![/]");
    AnsiConsole.MarkupLine($"  {Markup.Escape(quiz.Name)}");
    Pause();
}

// ── Helpers ───────────────────────────────────────────────────────────────────

// Returns null if user picks "← Back" or no courses exist.
static Course? PromptCourse(DataManager dm, string title)
{
    if (dm.Courses.Count == 0)
    {
        AnsiConsole.MarkupLine("[yellow]No courses found. Create a course first.[/]");
        Pause();
        return null;
    }

    const string BACK = "← Back";
    var labels = dm.Courses.Select(CourseDisplay).Append(BACK).ToList();
    var sel = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title(title)
            .UseConverter(s => s == BACK ? s : Markup.Escape(s))
            .AddChoices(labels)
    );
    if (sel == BACK) return null;
    return dm.Courses.First(c => CourseDisplay(c) == sel);
}

// Returns null if user picks "← Back" or course has no topics.
static Topic? PromptTopic(DataManager dm, Course course, string title)
{
    const string BACK = "← Back";
    var topics = dm.Topics.Where(t => t.CourseId == course.CourseId).ToList();

    if (topics.Count == 0)
    {
        AnsiConsole.MarkupLine("[yellow]No topics for this course.[/]");
        Pause();
        return null;
    }

    var labels = topics.Select(t => t.Name).Append(BACK).ToList();
    var sel = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title(title)
            .UseConverter(s => s == BACK ? s : Markup.Escape(s))
            .AddChoices(labels)
    );
    if (sel == BACK) return null;
    return topics.First(t => t.Name == sel);
}

// Returns null if user picks "← Back" or no quizzes exist.
static Quiz? PromptQuiz(DataManager dm, string title)
{
    if (dm.Quizzes.Count == 0)
    {
        AnsiConsole.MarkupLine("[yellow]No quizzes found. Create a quiz first.[/]");
        Pause();
        return null;
    }

    var sorted = dm.Quizzes.OrderBy(q => q.DueDate).ToList();
    const string BACK = "← Back";
    var labels = sorted.Select(q =>
    {
        string status = q.IsCompleted ? "✓" : " ";
        return $"[{status}] {q.Name}  (due {q.DueDate:yyyy-MM-dd})";
    }).Append(BACK).ToList();

    var sel = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title(title)
            .UseConverter(s => s == BACK ? s : Markup.Escape(s))
            .AddChoices(labels)
    );
    if (sel == BACK) return null;
    return sorted[labels.IndexOf(sel)];
}

// Returns null if user picks "← Back" or course has no reference lists.
static ReferenceList? PromptReferenceList(DataManager dm, Course course, string title)
{
    const string BACK = "← Back";
    var lists = dm.ReferenceLists.Where(r => r.CourseId == course.CourseId).ToList();

    if (lists.Count == 0)
    {
        AnsiConsole.MarkupLine("[yellow]No reference lists for this course. Create one first.[/]");
        Pause();
        return null;
    }

    var labels = lists.Select(r => r.Name).Append(BACK).ToList();
    var sel = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title(title)
            .UseConverter(s => s == BACK ? s : Markup.Escape(s))
            .AddChoices(labels)
    );
    if (sel == BACK) return null;
    return lists.First(r => r.Name == sel);
}

// Returns null if user picks "← Back" or list has no entries.
static ReferenceEntry? PromptReferenceEntry(DataManager dm, ReferenceList rl, string title)
{
    const string BACK = "← Back";
    var entries = dm.ReferenceEntries.Where(e => e.ReferenceListId == rl.ReferenceListId).ToList();

    if (entries.Count == 0)
    {
        AnsiConsole.MarkupLine("[yellow]No entries in this list.[/]");
        Pause();
        return null;
    }

    var labels = entries.Select(e =>
    {
        string def = e.Definition.Length > 40 ? e.Definition[..40] + "…" : e.Definition;
        return $"{e.Term} — {def}";
    }).Append(BACK).ToList();

    var sel = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title(title)
            .UseConverter(s => s == BACK ? s : Markup.Escape(s))
            .AddChoices(labels)
    );
    if (sel == BACK) return null;
    return entries[labels.IndexOf(sel)];
}

static void Pause() =>
    AnsiConsole.Prompt(new TextPrompt<string>("[grey]Press Enter to return...[/]").AllowEmpty());

static string CourseDisplay(Course course) =>
    $"{course.Name} ({course.SubjectArea})";
