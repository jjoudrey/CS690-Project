using Spectre.Console;
using static MiaLearningSystem.UiHelpers;

namespace MiaLearningSystem;

public static class TopicModule
{
    public static void OrganizeTopics(DataManager dm)
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

    public static void AddTopic(DataManager dm)
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

    public static void ViewTopics(DataManager dm)
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

    public static void EditTopic(DataManager dm)
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

    public static void DeleteTopic(DataManager dm)
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
}
