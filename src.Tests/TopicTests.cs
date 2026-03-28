using MiaLearningSystem;

public class TopicTests
{
    public TopicTests()
    {
        File.Delete("courses.txt");
        File.Delete("topics.txt");
        File.Delete("notes.txt");
        File.Delete("quizzes.txt");
        File.Delete("quiz_topics.txt");
        File.Delete("reference_lists.txt");
        File.Delete("reference_entries.txt");
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
}
