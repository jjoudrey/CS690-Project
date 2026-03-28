using Spectre.Console;

namespace MiaLearningSystem;

public static class UiHelpers
{
    public static Course? PromptCourse(DataManager dm, string title)
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

    public static Topic? PromptTopic(DataManager dm, Course course, string title)
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

    public static Quiz? PromptQuiz(DataManager dm, string title)
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

    public static ReferenceList? PromptReferenceList(DataManager dm, Course course, string title)
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

    public static ReferenceEntry? PromptReferenceEntry(DataManager dm, ReferenceList rl, string title)
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

    public static void Pause() =>
        AnsiConsole.Prompt(new TextPrompt<string>("[grey]Press Enter to return...[/]").AllowEmpty());

    public static string CourseDisplay(Course course) =>
        $"{course.Name} ({course.SubjectArea})";
}
