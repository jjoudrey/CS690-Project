using MiaLearningSystem;

public class TopicTests
{
    public TopicTests()
    {
        if (Directory.Exists("Data")) Directory.Delete("Data", recursive: true);
    }

    [Fact]
    public void Test_AddTopic()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Spanish 101", "Language");
        dm.AddCourse(course);
        dm.AddTopic(new Topic(Guid.NewGuid(), course.CourseId, "Verbs"));
        Assert.Equal(1, dm.Topics.Count);
    }

    [Fact]
    public void Test_UpdateTopic()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Spanish 101", "Language");
        dm.AddCourse(course);
        var topic = new Topic(Guid.NewGuid(), course.CourseId, "Old Name");
        dm.AddTopic(topic);
        dm.UpdateTopic(topic.TopicId, "New Name");
        Assert.Equal("New Name", dm.Topics[0].Name);
    }

    [Fact]
    public void Test_RemoveTopic()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Spanish 101", "Language");
        dm.AddCourse(course);
        var topic = new Topic(Guid.NewGuid(), course.CourseId, "Verbs");
        dm.AddTopic(topic);
        Assert.Equal(1, dm.Topics.Count);
        dm.RemoveTopic(topic);
        Assert.Equal(0, dm.Topics.Count);
    }

    [Fact]
    public void Test_PersistsTopics()
    {
        var dm1 = new DataManager();
        var course = new Course(Guid.NewGuid(), "Spanish 101", "Language");
        dm1.AddCourse(course);
        dm1.AddTopic(new Topic(Guid.NewGuid(), course.CourseId, "Verbs"));
        dm1.AddTopic(new Topic(Guid.NewGuid(), course.CourseId, "Nouns"));

        var dm2 = new DataManager();
        Assert.Equal(2, dm2.Topics.Count);
        Assert.Contains(dm2.Topics, t => t.Name == "Verbs");
        Assert.Contains(dm2.Topics, t => t.Name == "Nouns");
    }
}
