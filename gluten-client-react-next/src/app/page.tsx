'use client'

import Image from "next/image";
import 'bootstrap/dist/css/bootstrap.css';
import Map from './map';
import Sidebar from './sidebar';
import { TopicGroupClass } from '../_model/model';

export default function Home() {
  const selectedTopicGroup = new TopicGroupClass(1, 2, "test", "test", [], "test", "test", "test", "test");
  return (
    <div className="grid grid-rows-[20px_1fr_20px] items-center justify-items-center min-h-screen p-8 pb-20 gap-16 sm:p-20 font-[family-name:var(--font-geist-sans)]">
      <main className="flex flex-col gap-8 row-start-2 items-center sm:items-start">
        <div className="App">
          <div className="container">
            <div className="window">
              <div className="master">

                <h1 className="text-danger">GF FB Indexer</h1>
                <Map />
              </div>
              <div className="detail">
                <Sidebar selectedTopicGroup={selectedTopicGroup} />
              </div>
            </div>
          </div>
        </div>
      </main>
      <footer className="row-start-3 flex gap-6 flex-wrap items-center justify-center">
      </footer>
    </div>
  );
}
