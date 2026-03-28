using Spectre.Console;
using static MiaLearningSystem.UiHelpers;

namespace MiaLearningSystem;

public static class CourseModule
{
    public static void ManageCourses(DataManager dm)
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

    public static void CreateCourse(DataManager dm)
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

    public static void ViewCourses(DataManager dm)
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

    public static void EditCourse(DataManager dm)
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

    public static void DeleteCourse(DataManager dm)
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
}
