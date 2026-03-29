using System.Globalization;
using Spectre.Console;
using static MiaLearningSystem.UiHelpers;

namespace MiaLearningSystem;

public static class AssessmentModule
{
    public static void TrackAssessments(DataManager dm)
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[bold blue]TRACK UPCOMING ASSESSMENTS[/]").RuleStyle("blue"));

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[bold]Select an option:[/]")
                    .AddChoices("Create Quiz", "View Upcoming Quizzes", "View Study Guide",
                                "View Quizzes", "Edit Quiz", "Delete Quiz", "← Back")
            );

            switch (choice)
            {
                case "Create Quiz":           CreateQuiz(dm); break;
                case "View Upcoming Quizzes": ViewUpcomingQuizzes(dm); break;
                case "View Study Guide":      ViewStudyGuide(dm); break;
                case "View Quizzes":          ViewQuizzes(dm); break;
                case "Edit Quiz":             EditQuiz(dm); break;
                case "Delete Quiz":           DeleteQuiz(dm); break;
                case "← Back": return;
            }
        }
    }

    public static void CreateQuiz(DataManager dm)
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

    public static void ViewUpcomingQuizzes(DataManager dm)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]UPCOMING QUIZZES[/]").RuleStyle("blue"));

        var today = DateOnly.FromDateTime(DateTime.Today);
        var upcoming = dm.Quizzes
            .Where(q => !q.IsCompleted && q.DueDate >= today)
            .OrderBy(q => q.DueDate)
            .ToList();

        if (upcoming.Count == 0)
        {
            AnsiConsole.MarkupLine("\n[yellow]No upcoming quizzes.[/]");
            Pause();
            return;
        }

        var table = new Table();
        table.AddColumn("Due Date");
        table.AddColumn("Quiz");
        table.AddColumn("Course");

        foreach (var q in upcoming)
        {
            var course = dm.Courses.FirstOrDefault(c => c.CourseId == q.CourseId);
            string courseDisplay = course != null ? CourseDisplay(course) : "Unknown";
            table.AddRow(q.DueDate.ToString("yyyy-MM-dd"), Markup.Escape(q.Name), Markup.Escape(courseDisplay));
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(table);
        Pause();
    }

    public static void ViewStudyGuide(DataManager dm)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]STUDY GUIDE[/]").RuleStyle("blue"));

        var quiz = PromptQuiz(dm, "\n[bold]Select a quiz:[/]");
        if (quiz is null) return;

        var course = dm.Courses.FirstOrDefault(c => c.CourseId == quiz.CourseId);
        var quizTopicIds = dm.QuizTopics
            .Where(qt => qt.QuizId == quiz.QuizId)
            .Select(qt => qt.TopicId)
            .ToHashSet();
        var scopeTopics = dm.Topics.Where(t => quizTopicIds.Contains(t.TopicId)).ToList();

        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold blue]STUDY GUIDE[/]").RuleStyle("blue"));
        AnsiConsole.MarkupLine($"\n[bold]{Markup.Escape(quiz.Name)}[/]");
        AnsiConsole.MarkupLine($"Course: {Markup.Escape(course != null ? CourseDisplay(course) : "Unknown")}");
        AnsiConsole.MarkupLine($"Due:    {quiz.DueDate:yyyy-MM-dd}");

        if (scopeTopics.Count == 0)
        {
            AnsiConsole.MarkupLine("\n[yellow]No topics linked to this quiz.[/]");
            Pause();
            return;
        }

        foreach (var topic in scopeTopics)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule($"[bold]{Markup.Escape(topic.Name)}[/]").RuleStyle("blue"));

            var notes = dm.Notes.Where(n => n.TopicId == topic.TopicId).ToList();
            if (notes.Count == 0)
            {
                AnsiConsole.MarkupLine("  [grey](No notes)[/]");
                continue;
            }

            foreach (var note in notes)
            {
                AnsiConsole.MarkupLine($"\n  [bold underline]{Markup.Escape(note.Name)}[/]");
                string[]? lines = DataManager.ReadNoteLines(note, out string err);
                if (lines is null)
                    AnsiConsole.MarkupLine($"    [red]{Markup.Escape(err)}[/]");
                else if (lines.Length == 0)
                    AnsiConsole.MarkupLine("    [grey](empty)[/]");
                else
                    foreach (var line in lines)
                        AnsiConsole.MarkupLine($"    {Markup.Escape(line)}");
            }
        }

        AnsiConsole.WriteLine();
        Pause();
    }

    public static void ViewQuizzes(DataManager dm)
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

    public static void ViewQuizDetail(DataManager dm, Quiz quiz)
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

    public static void EditQuiz(DataManager dm)
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

    public static void DeleteQuiz(DataManager dm)
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
}
