require "rimtool/rake_tasks"
require "net/http"

desc "xml"
task :xml do
  mod = RimTool::Mod.new "."
  builder = Nokogiri::XML::Builder.new do |xml|
    xml.root("Class" => "zed_0xff.YADA.API.Request_SetItemDescription") {
      xml.PublishedFileId mod.id
      xml.description mod.readme.to_steam
    }
  end
  xml = builder.to_xml
  resp = Net::HTTP.post(URI('http://127.0.0.1:8192/'), xml)
  puts resp.body
end

task :foo do
  mod = RimTool::Mod.new "."
  builder = Nokogiri::XML::Builder.new do |xml|
    xml.root("Class" => "zed_0xff.YADA.API.Request_SendQueryUGCRequest") {
      xml.PublishedFileIds {
        xml.li mod.id
      }
    }
  end
  xml = builder.to_xml
  resp = Net::HTTP.post(URI('http://127.0.0.1:8192/'), xml)
  puts resp.body
end
