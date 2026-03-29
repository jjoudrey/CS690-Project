using MiaLearningSystem;

public class AssessmentTests
{
    public AssessmentTests()
    {
        if (Directory.Exists("Data")) Directory.Delete("Data", recursive: true);
    }

    // FR-12: upcoming quizzes filtering

    [Fact]
    public void Test_UpcomingQuizzes_FiltersFutureOnly()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Math 101", "Math");
        dm.AddCourse(course);

        var today = DateOnly.FromDateTime(DateTime.Today);
        dm.AddQuiz(new Quiz(Guid.NewGuid(), course.CourseId, "Past Quiz",   today.AddDays(-1), false));
        dm.AddQuiz(new Quiz(Guid.NewGuid(), course.CourseId, "Future Quiz", today.AddDays(7),  false));

        var upcoming = dm.Quizzes
            .Where(q => !q.IsCompleted && q.DueDate >= today)
            .ToList();

        Assert.Equal(1, upcoming.Count);
        Assert.Equal("Future Quiz", upcoming[0].Name);
    }

    [Fact]
    public void Test_UpcomingQuizzes_ExcludesCompleted()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Math 101", "Math");
        dm.AddCourse(course);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var quiz = new Quiz(Guid.NewGuid(), course.CourseId, "Upcoming Quiz", today.AddDays(5), false);
        dm.AddQuiz(quiz);
        dm.MarkQuizCompleted(quiz.QuizId);

        var upcoming = dm.Quizzes
            .Where(q => !q.IsCompleted && q.DueDate >= today)
            .ToList();

        Assert.Equal(0, upcoming.Count);
    }

    [Fact]
    public void Test_UpcomingQuizzes_SortsByDate()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Math 101", "Math");
        dm.AddCourse(course);

        var today = DateOnly.FromDateTime(DateTime.Today);
        dm.AddQuiz(new Quiz(Guid.NewGuid(), course.CourseId, "Later Quiz",  today.AddDays(14), false));
        dm.AddQuiz(new Quiz(Guid.NewGuid(), course.CourseId, "Sooner Quiz", today.AddDays(3),  false));
        dm.AddQuiz(new Quiz(Guid.NewGuid(), course.CourseId, "Middle Quiz", today.AddDays(7),  false));

        var upcoming = dm.Quizzes
            .Where(q => !q.IsCompleted && q.DueDate >= today)
            .OrderBy(q => q.DueDate)
            .ToList();

        Assert.Equal(3, upcoming.Count);
        Assert.Equal("Sooner Quiz", upcoming[0].Name);
        Assert.Equal("Middle Quiz", upcoming[1].Name);
        Assert.Equal("Later Quiz",  upcoming[2].Name);
    }

    // FR-13: study guide data retrieval

    [Fact]
    public void Test_StudyGuide_TopicsLinkedToQuiz()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Bio 101", "Science");
        dm.AddCourse(course);

        var topic1       = new Topic(Guid.NewGuid(), course.CourseId, "Cells");
        var topic2       = new Topic(Guid.NewGuid(), course.CourseId, "DNA");
        var unlinkedTopic = new Topic(Guid.NewGuid(), course.CourseId, "Osmosis");
        dm.AddTopic(topic1);
        dm.AddTopic(topic2);
        dm.AddTopic(unlinkedTopic);

        var quiz = new Quiz(Guid.NewGuid(), course.CourseId, "Midterm",
                            DateOnly.FromDateTime(DateTime.Today).AddDays(7), false);
        dm.AddQuiz(quiz);
        dm.AddQuizTopic(new QuizTopic(Guid.NewGuid(), quiz.QuizId, topic1.TopicId));
        dm.AddQuizTopic(new QuizTopic(Guid.NewGuid(), quiz.QuizId, topic2.TopicId));

        var linkedIds   = dm.QuizTopics.Where(qt => qt.QuizId == quiz.QuizId).Select(qt => qt.TopicId).ToHashSet();
        var scopeTopics = dm.Topics.Where(t => linkedIds.Contains(t.TopicId)).ToList();

        Assert.Equal(2, scopeTopics.Count);
        Assert.Contains(scopeTopics, t => t.Name == "Cells");
        Assert.Contains(scopeTopics, t => t.Name == "DNA");
        Assert.DoesNotContain(scopeTopics, t => t.Name == "Osmosis");
    }

    [Fact]
    public void Test_UpdateQuiz()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Math 101", "Math");
        dm.AddCourse(course);
        var quiz = new Quiz(Guid.NewGuid(), course.CourseId, "Old Name",
                            DateOnly.FromDateTime(DateTime.Today).AddDays(7), false);
        dm.AddQuiz(quiz);

        var newDate = DateOnly.FromDateTime(DateTime.Today).AddDays(14);
        dm.UpdateQuiz(quiz.QuizId, "New Name", newDate);

        Assert.Equal("New Name", dm.Quizzes[0].Name);
        Assert.Equal(newDate, dm.Quizzes[0].DueDate);
    }

    [Fact]
    public void Test_RemoveQuiz_CascadesTopics()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Math 101", "Math");
        dm.AddCourse(course);
        var topic = new Topic(Guid.NewGuid(), course.CourseId, "Algebra");
        dm.AddTopic(topic);
        var quiz = new Quiz(Guid.NewGuid(), course.CourseId, "Midterm",
                            DateOnly.FromDateTime(DateTime.Today).AddDays(7), false);
        dm.AddQuiz(quiz);
        dm.AddQuizTopic(new QuizTopic(Guid.NewGuid(), quiz.QuizId, topic.TopicId));
        Assert.Equal(1, dm.QuizTopics.Count);

        dm.RemoveQuiz(quiz);

        Assert.Equal(0, dm.Quizzes.Count);
        Assert.Equal(0, dm.QuizTopics.Count);
    }

    [Fact]
    public void Test_ReplaceQuizTopics()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Math 101", "Math");
        dm.AddCourse(course);
        var topic1 = new Topic(Guid.NewGuid(), course.CourseId, "Algebra");
        var topic2 = new Topic(Guid.NewGuid(), course.CourseId, "Calculus");
        dm.AddTopic(topic1);
        dm.AddTopic(topic2);
        var quiz = new Quiz(Guid.NewGuid(), course.CourseId, "Final",
                            DateOnly.FromDateTime(DateTime.Today).AddDays(14), false);
        dm.AddQuiz(quiz);
        dm.AddQuizTopic(new QuizTopic(Guid.NewGuid(), quiz.QuizId, topic1.TopicId));
        Assert.Equal(1, dm.QuizTopics.Count);

        dm.ReplaceQuizTopics(quiz.QuizId, [topic2.TopicId]);

        Assert.Equal(1, dm.QuizTopics.Count);
        Assert.Equal(topic2.TopicId, dm.QuizTopics[0].TopicId);
    }

    [Fact]
    public void Test_PersistsQuizzes()
    {
        var dm1 = new DataManager();
        var course = new Course(Guid.NewGuid(), "Math 101", "Math");
        dm1.AddCourse(course);
        var due = DateOnly.FromDateTime(DateTime.Today).AddDays(7);
        dm1.AddQuiz(new Quiz(Guid.NewGuid(), course.CourseId, "Midterm", due, false));

        var dm2 = new DataManager();
        Assert.Equal(1, dm2.Quizzes.Count);
        Assert.Equal("Midterm", dm2.Quizzes[0].Name);
        Assert.Equal(due, dm2.Quizzes[0].DueDate);
        Assert.False(dm2.Quizzes[0].IsCompleted);
    }

    [Fact]
    public void Test_PersistsQuizTopics()
    {
        var dm1 = new DataManager();
        var course = new Course(Guid.NewGuid(), "Math 101", "Math");
        dm1.AddCourse(course);
        var topic = new Topic(Guid.NewGuid(), course.CourseId, "Algebra");
        dm1.AddTopic(topic);
        var quiz = new Quiz(Guid.NewGuid(), course.CourseId, "Final",
                            DateOnly.FromDateTime(DateTime.Today).AddDays(14), false);
        dm1.AddQuiz(quiz);
        dm1.AddQuizTopic(new QuizTopic(Guid.NewGuid(), quiz.QuizId, topic.TopicId));

        var dm2 = new DataManager();
        Assert.Equal(1, dm2.QuizTopics.Count);
        Assert.Equal(quiz.QuizId, dm2.QuizTopics[0].QuizId);
        Assert.Equal(topic.TopicId, dm2.QuizTopics[0].TopicId);
    }

    [Fact]
    public void Test_StudyGuide_NotesForLinkedTopics()
    {
        var dm = new DataManager();
        var course = new Course(Guid.NewGuid(), "Bio 101", "Science");
        dm.AddCourse(course);

        var topic = new Topic(Guid.NewGuid(), course.CourseId, "Cells");
        dm.AddTopic(topic);

        var quiz = new Quiz(Guid.NewGuid(), course.CourseId, "Midterm",
                            DateOnly.FromDateTime(DateTime.Today).AddDays(7), false);
        dm.AddQuiz(quiz);
        dm.AddQuizTopic(new QuizTopic(Guid.NewGuid(), quiz.QuizId, topic.TopicId));

        dm.AddNote(new Note(Guid.NewGuid(), topic.TopicId, "note1.txt", "Cell Structure"));
        dm.AddNote(new Note(Guid.NewGuid(), topic.TopicId, "note2.txt", "Cell Function"));

        var linkedIds = dm.QuizTopics.Where(qt => qt.QuizId == quiz.QuizId).Select(qt => qt.TopicId).ToHashSet();
        var notes     = dm.Notes.Where(n => linkedIds.Contains(n.TopicId)).ToList();

        Assert.Equal(2, notes.Count);
        Assert.Contains(notes, n => n.Name == "Cell Structure");
        Assert.Contains(notes, n => n.Name == "Cell Function");
    }
}
