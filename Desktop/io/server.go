package main

import (
    "fmt"
    "html/template"
    "net/http"
    //"strings"
    "log"
    "io/ioutil" 
    "encoding/json"  
    //"regexp"
    //"os"
)

type JSON struct {  
    Id 	      string `json:"ID"`  
    Prefix    string `json:"prefix"`  
    Province  string `json:"province"`  
    City      string `json:"city"`  
    Operator  string `json:"operator"`  
    AreaCode  string `json:"areaCode"` 
    Zip       string `json:"zip"` 
    Ret       int    `json:"ret"` 
    SearchStr string `json:"searchStr"` 
    From      string `json:"from"` 
}  



func api(w http.ResponseWriter, num string) *JSON{
    resp, err := http.Get(fmt.Sprintf("https://www.iteblog.com/api/mobile.php?mobile=%s", num))  //向API请求数据
    if err != nil {  
        return nil  
    }  
    defer resp.Body.Close()  

    out, err := ioutil.ReadAll(resp.Body)  		//解释返回数据
    if err != nil {  
        return nil  
    }  

    var result JSON

    if err := json.Unmarshal(out, &result); err != nil {  	//解释JSON
        return nil  
    }  
   

    return &result  
}



func main() {

	h := http.FileServer(http.Dir("static"))
	http.Handle("/static/", http.StripPrefix("/static/", h)) 
	http.HandleFunc("/", handler)
	http.ListenAndServe(":8080", nil)

}

func handler(w http.ResponseWriter, r *http.Request) {
	if r.URL.RequestURI() == "/" {
		load(w, r)
	} else {
		notFound(w, r)
	}
}


func notFound(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "text/html;charset=gb2312")
	w.Write([]byte("5xx Unknow page \"" + r.URL.RequestURI() + "\" access"))
	w.WriteHeader(505)
}



func load(w http.ResponseWriter, r *http.Request) {
	if r.Method == "GET" {
		Template, err := template.ParseFiles("index.html")
		if err != nil {
			log.Fatal(err)
		}

		data := struct {
			Title string
		}{
			Title: "Number Page",
		}

		err = Template.Execute(w, data)

		if err != nil {
			log.Fatal(err)
		}
	} else if r.Method == "POST" {
		err := r.ParseForm()
		if err != nil {
			panic(err)
		}


                result := api(w, r.Form.Get("tel"))
		w.Header().Set("Content-Type", "charset=gb2312;text/html")
		w.Write([]byte("归属地:" + result.Province + result.City + "\n手机服务商:" + result.Operator))

	}
}
