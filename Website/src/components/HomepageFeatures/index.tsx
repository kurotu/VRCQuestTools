import React from 'react';
import clsx from 'clsx';
import styles from './styles.module.css';

type FeatureItem = {
  title: string;
  description: JSX.Element;
};

const FeatureList: FeatureItem[] = [
  {
    title: 'One-Click Conversion',
    description: (
      <>
        VRCQuestTools automatically makes your avatar uploadable for Quest.
        You are free from annoying manual work.
      </>
    ),
  },
  {
    title: 'Non Destructive',
    description: (
      <>
        VRCQuestTools keeps existing avatars and assets while conversion. Don't have to worry about unexpected modification.
      </>
    ),
  },
  {
    title: 'Misc Utilities',
    description: (
      <>
        VRCQuestTools has various features to help uploading Quest avatars.
      </>
    ),
  },
];

function Feature({title, description}: FeatureItem) {
  return (
    <div className={clsx('col col--4')}>
      <div className="text--center padding-horiz--md">
        <h3>{title}</h3>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageFeatures(): JSX.Element {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
