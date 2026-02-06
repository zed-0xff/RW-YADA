# https://github.com/zed-0xff/rimtool
require "rimtool/rake_tasks"

Dir["./lib/tasks/*.rake"].each{ |fn| load fn }

Rake::Task[:build].clear

task :build do
  Dir.chdir "Source"
  sh "dotnet build -c Release -p:RimWorldVersion=1.5"
  Dir.chdir ".."
end
