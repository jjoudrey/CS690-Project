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
}
