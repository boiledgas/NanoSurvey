CREATE SCHEMA survey
GO

-- сущность вопрос
CREATE TABLE survey.Question (
	Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_survey_Question PRIMARY KEY
	,Title NVARCHAR(max) NOT NULL
	,HELP NVARCHAR(max) NOT NULL
	)
GO

-- сущность вариант ответа на вопрос
CREATE TABLE survey.Answer (
	Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_survey_Answer PRIMARY KEY
	,Title NVARCHAR(255) NOT NULL
	)
GO

-- сущность анкета
CREATE TABLE survey.Survey (
	Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_survey_Survey PRIMARY KEY
	,NAME NVARCHAR(255) NOT NULL
	)
GO

-- сущность вопрос анкеты
CREATE TABLE survey.SurveyQuestion (
	Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_survey_SurveyQuestion PRIMARY KEY
	,SurveyId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_survey_SurveyQuestion_SurveyId FOREIGN KEY REFERENCES survey.Survey(id)
	,QuestionId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_survey_SurveyQuestion_QuestionId FOREIGN KEY REFERENCES survey.Question(id)
	,NextQuestionId UNIQUEIDENTIFIER NULL CONSTRAINT FK_survey_SurveyQuestionRoute_NextQuestionId FOREIGN KEY REFERENCES survey.Question(id)
	,PreviousQuestionId UNIQUEIDENTIFIER NULL CONSTRAINT FK_survey_SurveyQuestionRoute_PreviousQuestionId FOREIGN KEY REFERENCES survey.Question(id)
	,Count INT NOT NULL 
	
	INDEX IX_SurveyQuestion_SurveyId NONCLUSTERED (SurveyId)
	)
GO

-- сущность вариантов ответов на вопрос анкеты
CREATE TABLE survey.SurveyQuestionAnswer (
	Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_survey_SurveyQuestionAnswer PRIMARY KEY
	,SurveyId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_survey_SurveyQuestionAnswer_SurveyId FOREIGN KEY REFERENCES survey.Survey(id)
	,QuestionId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_survey_SurveyQuestionAnswer_QuestionId FOREIGN KEY REFERENCES survey.Question(id)
	,AnswerId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_survey_SurveyQuestionAnswer_AnswerId FOREIGN KEY REFERENCES survey.Answer(id)
	,OrderNum INT NOT NULL 
	
	INDEX IX_SurveyQuestionAnswer_QuestionId NONCLUSTERED (SurveyId)
	)
GO

CREATE SCHEMA result
GO

-- Сущность интервью
CREATE TABLE result.Interview (
	Id BIGINT NOT NULL identity(1, 1) CONSTRAINT PK_result_Interview PRIMARY KEY
	,ExternalId UNIQUEIDENTIFIER NOT NULL
	,SurveyId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_result_Interview_SurveyId FOREIGN KEY REFERENCES survey.Survey(id)
	)
GO

-- Сущность ответ на вопрос
CREATE TABLE result.Result (
	Id BIGINT NOT NULL identity(1, 1) CONSTRAINT PK_result_Result PRIMARY KEY
	,InterviewId BIGINT NOT NULL CONSTRAINT FK_result_Result_InterviewId FOREIGN KEY REFERENCES result.Interview(id)
	,QuestionId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_result_Result_QuestionId FOREIGN KEY REFERENCES survey.Question(id)
	)
GO

-- Сущность ответ на вопрос
CREATE TABLE result.ResultAnswer (
	Id BIGINT NOT NULL identity(1, 1) CONSTRAINT PK_result_ResultAnswer PRIMARY KEY
	,ResultId BIGINT NOT NULL CONSTRAINT FK_result_ResultAnswer_ResultId FOREIGN KEY REFERENCES result.Result(id)
	,AnswerId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_result_ResultAnswer_AnswerId FOREIGN KEY REFERENCES survey.Answer(id)
	)
GO
CREATE NONCLUSTERED INDEX IX_result_Result_InterviewId ON result.Result (InterviewId) INCLUDE (QuestionId)
GO
CREATE NONCLUSTERED INDEX IX_result_ResultAnswer_ResultId ON result.ResultAnswer (ResultId) INCLUDE (AnswerId)
GO



