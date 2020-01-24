dotnet user-secrets set ConnectionStrings:NanoSurvey %1 --project NanoSurvey.Web
dotnet user-secrets set ConnectionStrings:NanoSurvey %1 --project NanoSurvey.Generator
dotnet ef dbcontext scaffold %1 Microsoft.EntityFrameworkCore.SqlServer --schema survey --project NanoSurvey.DB --force --data-annotations -c "SurveyContext"
dotnet ef dbcontext scaffold %1 Microsoft.EntityFrameworkCore.SqlServer --schema result --project NanoSurvey.DB --force --data-annotations -c "ResultContext"