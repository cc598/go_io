package main

import (
    "fmt"
    "html/template"
    "net/http"
    "strings"
    "log"
    "io/ioutil" 
    "encoding/json"  
    "regexp"
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

func sayhelloName(w http.ResponseWriter, r *http.Request) {
    r.ParseForm()  //解析参数，默认是不会解析的
    fmt.Println(r.Form)  //这些信息是输出到服务器端的打印信息
    fmt.Println("path", r.URL.Path)
    fmt.Println("scheme", r.URL.Scheme)
    fmt.Println(r.Form["url_long"])
    for k, v := range r.Form {
        fmt.Println("key:", k)
        fmt.Println("val:", strings.Join(v, ""))
    }				

    if m, _ := regexp.MatchString(`^(1[3|4|5|8][0-9]\d{4,8})$`, r.Form.Get("tel")); !m {	//检验输入的正确性
        fmt.Fprintf(w, "手机号码错误！")
    } else {
        result := api(w, r.Form.Get("tel"))
        fmt.Fprintf(w, "归属地:%s%s\n手机服务商:%s", result.Province, result.City, result.Operator)	//输出结果
    }
}

func home(w http.ResponseWriter, r *http.Request) {    
	fmt.Println("method:", r.Method) //获取请求的方法    
	if r.Method == "GET" {        
		t, _ := template.ParseFiles("home.gtpl")        
		t.Execute(w, nil)    
	} else {        //请求的是登陆数据，那么执行登陆的逻辑判断
		r.ParseForm();        
		fmt.Println("tel:", r.Form["tel"])        
	} 
} 

func main() {
    http.HandleFunc("/", sayhelloName)       //设置访问的路由
    http.HandleFunc("/home", home)         //设置访问的路
    err := http.ListenAndServe(":9090", nil) //设置监听的端口
    if err != nil {
        log.Fatal("ListenAndServe: ", err)
    }
}


