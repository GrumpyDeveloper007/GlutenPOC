import { Component } from '@angular/core';
import { NgIf } from '@angular/common';
import { Topic } from "../model/model";
import { Input } from '@angular/core';


@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [NgIf],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css'
})
export class SidebarComponent {
  @Input() selectedTopic: Topic | null = null;
  facebookLink = 'about:blank';
  //selectedTopic: Topic | null = null;

}