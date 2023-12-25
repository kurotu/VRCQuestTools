import React from 'react';
import clsx from 'clsx';
import styles from './styles.module.css';
import Translate from '@docusaurus/Translate';

type FeatureItem = {
  title: JSX.Element;
  description: JSX.Element;
};

const FeatureList: FeatureItem[] = [
  {
    title: <Translate>Easy Avatar Conversion</Translate>,
    description: (
      <Translate>
        VRCQuestTools automates works to make your avatar uploadable for Android.
        You are free from annoying manual work.
      </Translate>
    ),
  },
  {
    title: <Translate>Non Destructive</Translate>,
    description: (
      <Translate>
        VRCQuestTools keeps existing avatars and assets while conversion. You can use the tool in existing projects as are.
      </Translate>
    ),
  },
  {
    title: <Translate>Misc Utilities</Translate>,
    description: (
      <Translate>
        VRCQuestTools has various features to help uploading Android avatars other than avatar convertersion.
      </Translate>
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
