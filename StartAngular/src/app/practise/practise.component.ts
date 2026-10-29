import { Component, ElementRef, ViewChild } from '@angular/core';

@Component({
  selector: 'app-practise',
  templateUrl: './practise.component.html',
  styleUrls: ['./practise.component.css']
})
export class PractiseComponent {

  @ViewChild('tempMessage') myInput!: ElementRef;
  name:string = "Angular";

  url:string = "https://angular.io/";

  Greet():void{
    alert("Hello, welcome to Angular!");
  }

  student:any[]=[
    {name:"shadab",Marks:90},
    {name:"Anil",Marks:80},
    {name:"Junaid",Marks:70},
    {name:"Gaurav",Marks:40},
    {name:"Jio",Marks:30},
    {name:"Suresh",Marks:35},
  ];

  userInputnumber: number | null = null;
  
  colors:any[]=['Violet','Indigo','Blue','Green','Violet','red'];

  color:string | null =null;

  Alphabet:string | null =null;

  displayMessage(){
    alert(this.myInput.nativeElement.value);
  }

  myFunction = () => {
    alert("This is a constant function");
  }

  inputvalue:number | null = null;

  squareValue=(number:number)=>(number*number);
}
