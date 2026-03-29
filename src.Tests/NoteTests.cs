using MiaLearningSystem;

public class NoteTests
{
    public NoteTests()
    {
        if (Directory.Exists("Data")) Directory.Delete("Data", recursive: true);
    }

    [Fact]
    public void Test_AddNote()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Spanish 101", "Language");
        dm.AddCourse(course);
        var topic = new Topic(Guid.NewGuid(), course.CourseId, "Verbs");
        dm.AddTopic(topic);
        dm.AddNote(new Note(Guid.NewGuid(), topic.TopicId, "file1.txt", "My First Note"));
        Assert.Equal(1, dm.Notes.Count);
    }

    [Fact]
    public void Test_RemoveNote()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Spanish 101", "Language");
        dm.AddCourse(course);
        var topic = new Topic(Guid.NewGuid(), course.CourseId, "Verbs");
        dm.AddTopic(topic);
        var note = new Note(Guid.NewGuid(), topic.TopicId, "file1.txt", "My First Note");
        dm.AddNote(note);
        Assert.Equal(1, dm.Notes.Count);
        dm.RemoveNote(note);
        Assert.Equal(0, dm.Notes.Count);
    }

    [Fact]
    public void Test_UpdateNote()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Spanish 101", "Language");
        dm.AddCourse(course);
        var topic = new Topic(Guid.NewGuid(), course.CourseId, "Verbs");
        dm.AddTopic(topic);
        var note = new Note(Guid.NewGuid(), topic.TopicId, "file1.txt", "Old Name");
        dm.AddNote(note);
        dm.UpdateNote(note.NoteId, "New Name");
        Assert.Equal("New Name", dm.Notes[0].Name);
    }

    [Fact]
    public void Test_AppendNoteLine()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Spanish 101", "Language");
        dm.AddCourse(course);
        var topic = new Topic(Guid.NewGuid(), course.CourseId, "Verbs");
        dm.AddTopic(topic);
        var note = new Note(Guid.NewGuid(), topic.TopicId, $"{Guid.NewGuid()}.txt", "My Note");
        dm.AddNote(note);

        DataManager.AppendNoteLine(note, "First line");
        DataManager.AppendNoteLine(note, "Second line");

        string[]? lines = DataManager.ReadNoteLines(note, out _);
        Assert.NotNull(lines);
        Assert.Equal(2, lines!.Length);
        Assert.Equal("First line", lines[0]);
        Assert.Equal("Second line", lines[1]);
    }

    [Fact]
    public void Test_ReadNoteLines_FileNotFound()
    {
        var note = new Note(Guid.NewGuid(), Guid.NewGuid(), "nonexistent.txt", "Ghost Note");
        // Ensure Data/Notes exists so ReadNoteLines can look for the file
        Directory.CreateDirectory(DataManager.NotesDirectory);

        string[]? lines = DataManager.ReadNoteLines(note, out string err);
        Assert.Null(lines);
        Assert.NotEmpty(err);
    }

    [Fact]
    public void Test_ReadNoteLines_NonAsciiReturnsNull()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Spanish 101", "Language");
        dm.AddCourse(course);
        var topic = new Topic(Guid.NewGuid(), course.CourseId, "Verbs");
        dm.AddTopic(topic);
        string fileName = $"{Guid.NewGuid()}.txt";
        var note = new Note(Guid.NewGuid(), topic.TopicId, fileName, "Non-ASCII Note");
        dm.AddNote(note);

        // Write a file with a non-ASCII byte (é = 0xE9 in Latin-1)
        string filePath = DataManager.NoteFilePath(note);
        File.WriteAllBytes(filePath, [0xE9, 0x0A]);

        string[]? lines = DataManager.ReadNoteLines(note, out string err);
        Assert.Null(lines);
        Assert.NotEmpty(err);
    }

    [Fact]
    public void Test_PersistsNotes()
    {
        var dm1 = new DataManager();
        var course = new Course(Guid.NewGuid(), "Spanish 101", "Language");
        dm1.AddCourse(course);
        var topic = new Topic(Guid.NewGuid(), course.CourseId, "Verbs");
        dm1.AddTopic(topic);
        dm1.AddNote(new Note(Guid.NewGuid(), topic.TopicId, "file1.txt", "Note One"));
        dm1.AddNote(new Note(Guid.NewGuid(), topic.TopicId, "file2.txt", "Note Two"));

        var dm2 = new DataManager();
        Assert.Equal(2, dm2.Notes.Count);
        Assert.Contains(dm2.Notes, n => n.Name == "Note One");
        Assert.Contains(dm2.Notes, n => n.Name == "Note Two");
    }
}
