require 'fileutils'
require 'set'

task default: [:release, :clean, :list]
task release: [:mod, :test]

desc "clean"
task :clean do
  system "find . -name obj -or -name bin | xargs rm -rf", exception: true
  my_name = File.basename(File.expand_path(".")) + ".dll"
  Dir["Assemblies/*"].each do |fname|
    File.unlink(fname) unless File.basename(fname) == my_name
  end
end

desc "build mod"
task :mod do
  Dir.chdir "Source"
  system "dotnet build -c Release", exception: true
  Dir.chdir ".."
end

desc "test"
task :test do
  Dir.chdir "Test"
  system "rake", exception: true
  Dir.chdir ".."
end

def list_dir dir, ignores = Set.new([".", ".."])
  if File.exist?(".rimignore")
    ignores = ignores.dup + Set.new(File.readlines(".rimignore").map{ |line| line[0] == "#" ? "" : line.strip }.uniq).delete("")
  end

  r = 0
  globs = ignores.find_all{ |i| i[/[?*]/] }
  Dir.foreach(dir).to_a
    .delete_if{ |fn| ignores.include?(fn) }
    .delete_if{ |fn| globs.any?{ |g| File.fnmatch(g, fn, File::FNM_DOTMATCH) } }
    .each do |fn|
      pathname = File.join(dir, fn)
      if File.directory?(pathname)
        r += list_dir(pathname)
      else
        fsize = File.size(pathname)
        printf "[.] %5d KB  %s\n", fsize/1024, pathname
        r += fsize
      end
    end
  r
end

task :list do
  printf "[=] %5d KB\n", list_dir(".")/1024
end
