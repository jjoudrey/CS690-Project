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
        case "Manage Courses":         CourseModule.ManageCourses(dm); break;
        case "Organize Topics":        TopicModule.OrganizeTopics(dm); break;
        case "Manage Study Notes":     NoteModule.ManageStudyNotes(dm); break;
        case "Manage Reference Lists": ReferenceListModule.ManageReferenceLists(dm); break;
        case "Track Assessments":      AssessmentModule.TrackAssessments(dm); break;
        case "Exit":
            AnsiConsole.MarkupLine("[grey]Goodbye![/]");
            return;
    }
}
